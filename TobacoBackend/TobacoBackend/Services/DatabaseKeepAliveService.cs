using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TobacoBackend.Services;

/// <summary>
/// Mantiene la conexión a la base de datos "caliente" ejecutando un ping periódico.
/// Reduce el cold start que vería el usuario tras minutos de inactividad (p. ej. Azure SQL, pool frío).
/// </summary>
public class DatabaseKeepAliveService : IHostedService, IDisposable
{
    private readonly ILogger<DatabaseKeepAliveService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(4);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public DatabaseKeepAliveService(
        ILogger<DatabaseKeepAliveService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DatabaseKeepAlive: iniciado (intervalo {Interval} min)", Interval.TotalMinutes);
        _timer = new Timer(PingDatabase, null, Interval, Interval);
        return Task.CompletedTask;
    }

    private async void PingDatabase(object? state)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AplicationDbContext>();
            using var cts = new CancellationTokenSource(Timeout);
            await db.Database.CanConnectAsync(cts.Token);
            _logger.LogDebug("DatabaseKeepAlive: ping OK");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DatabaseKeepAlive: ping falló (no crítico)");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
