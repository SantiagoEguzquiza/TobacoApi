using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TobacoBackend.Services
{
    /// <summary>
    /// Servicio para realizar backups automáticos de la base de datos
    /// </summary>
    public class BackupService : IHostedService, IDisposable
    {
        private readonly ILogger<BackupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private Timer? _timer;
        private readonly TimeSpan _backupInterval;

        public BackupService(
            ILogger<BackupService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            
            // Configurar intervalo de backup (por defecto 24 horas)
            var hours = _configuration.GetValue<int>("BackupSettings:IntervalHours", 24);
            _backupInterval = TimeSpan.FromHours(hours);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackupService iniciado. Intervalo de backup: {Interval}", _backupInterval);
            
            // Ejecutar backup inmediatamente al iniciar (opcional)
            var runOnStart = _configuration.GetValue<bool>("BackupSettings:RunOnStart", false);
            if (runOnStart)
            {
                _ = Task.Run(() => PerformBackupAsync(cancellationToken), cancellationToken);
            }

            // Programar backups periódicos
            _timer = new Timer(ExecuteBackup, null, _backupInterval, _backupInterval);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackupService detenido.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void ExecuteBackup(object? state)
        {
            _ = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var cancellationToken = CancellationToken.None;
                await PerformBackupAsync(cancellationToken);
            });
        }

        public async Task PerformBackupAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando backup de base de datos...");
                var stopwatch = Stopwatch.StartNew();

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AplicationDbContext>();
                var connectionString = dbContext.Database.GetConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("No se pudo obtener la cadena de conexión para el backup.");
                    return;
                }

                // Obtener configuración de backup
                var backupPath = _configuration.GetValue<string>("BackupSettings:Path") 
                    ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                
                var retentionDays = _configuration.GetValue<int>("BackupSettings:RetentionDays", 30);
                var maxBackups = _configuration.GetValue<int>("BackupSettings:MaxBackups", 10);

                // Crear directorio si no existe
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                    _logger.LogInformation("Directorio de backups creado: {Path}", backupPath);
                }

                // Generar nombre de archivo con timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var databaseName = ExtractDatabaseName(connectionString);
                var backupFileName = $"Tobaco_{databaseName}_{timestamp}.bak";
                var backupFilePath = Path.Combine(backupPath, backupFileName);

                // Ejecutar backup usando sqlcmd o SMO
                var success = await ExecuteSqlBackup(connectionString, backupFilePath, databaseName, cancellationToken);

                if (success)
                {
                    stopwatch.Stop();
                    var fileSize = new FileInfo(backupFilePath).Length / (1024.0 * 1024.0); // MB
                    _logger.LogInformation(
                        "Backup completado exitosamente. Archivo: {FileName}, Tamaño: {Size:F2} MB, Tiempo: {Elapsed}ms",
                        backupFileName, fileSize, stopwatch.ElapsedMilliseconds);

                    // Limpiar backups antiguos
                    CleanupOldBackups(backupPath, retentionDays, maxBackups);
                }
                else
                {
                    _logger.LogError("El backup falló. Verificar logs anteriores para más detalles.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar backup de base de datos.");
            }
        }

        private string ExtractDatabaseName(string connectionString)
        {
            // Extraer nombre de base de datos de la connection string
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                {
                    return part.Split('=')[1].Trim();
                }
            }
            return "Tobaco";
        }

        private async Task<bool> ExecuteSqlBackup(string connectionString, string backupFilePath, string databaseName, CancellationToken cancellationToken = default)
        {
            try
            {
                // Ejecutar backup usando DbContext
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AplicationDbContext>();
                
                // Usar ExecuteSqlRaw para ejecutar el comando de backup
                // Nota: Requiere permisos de sysadmin o backup operator en SQL Server
                // Escapar las barras invertidas en la ruta para SQL
                var escapedPath = backupFilePath.Replace("'", "''").Replace("\\", "\\\\");
                var sqlCommand = $"BACKUP DATABASE [{databaseName}] TO DISK = '{escapedPath}' WITH FORMAT, INIT, NAME = 'Tobaco Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";
                
                // Usar ExecuteSqlRaw (síncrono) ya que BACKUP es un comando que no retorna resultados
                // y puede ejecutarse de forma síncrona sin problemas
                dbContext.Database.ExecuteSqlRaw(sqlCommand);

                // Esperar un momento para que el archivo se cree
                await Task.Delay(1000);

                return File.Exists(backupFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar comando de backup SQL.");
                
                // Fallback: Intentar con sqlcmd si está disponible
                return await TrySqlCmdBackup(connectionString, backupFilePath, databaseName);
            }
        }

        private string ExtractServerName(string connectionString)
        {
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.Trim().StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                {
                    return part.Split('=')[1].Trim();
                }
            }
            return "localhost";
        }

        private async Task<bool> TrySqlCmdBackup(string connectionString, string backupFilePath, string databaseName)
        {
            try
            {
                var serverName = ExtractServerName(connectionString);
                var sqlCmdPath = FindSqlCmdPath();

                if (string.IsNullOrEmpty(sqlCmdPath))
                {
                    _logger.LogWarning("sqlcmd no encontrado. El backup requiere sqlcmd o permisos de SQL Server.");
                    return false;
                }

                var arguments = $"-S {serverName} -E -Q \"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFilePath}' WITH FORMAT, INIT, NAME = 'Tobaco Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10\"";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = sqlCmdPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processStartInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    return process.ExitCode == 0 && File.Exists(backupFilePath);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar backup con sqlcmd.");
                return false;
            }
        }

        private string? FindSqlCmdPath()
        {
            var possiblePaths = new[]
            {
                @"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe",
                @"C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqlcmd.exe",
                @"C:\Program Files\Microsoft SQL Server\140\Tools\Binn\sqlcmd.exe",
                @"C:\Program Files (x86)\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe",
                @"C:\Program Files (x86)\Microsoft SQL Server\150\Tools\Binn\sqlcmd.exe",
            };

            return possiblePaths.FirstOrDefault(File.Exists);
        }

        private void CleanupOldBackups(string backupPath, int retentionDays, int maxBackups)
        {
            try
            {
                var files = Directory.GetFiles(backupPath, "Tobaco_*.bak")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                // Eliminar backups más antiguos que retentionDays
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
                var oldFiles = files.Where(f => f.CreationTime < cutoffDate).ToList();

                foreach (var file in oldFiles)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogInformation("Backup antiguo eliminado: {FileName}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo eliminar backup antiguo: {FileName}", file.Name);
                    }
                }

                // Mantener solo maxBackups más recientes
                if (files.Count > maxBackups)
                {
                    var filesToDelete = files.Skip(maxBackups).ToList();
                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            file.Delete();
                            _logger.LogInformation("Backup eliminado (límite excedido): {FileName}", file.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "No se pudo eliminar backup: {FileName}", file.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar backups antiguos.");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

