using TobacoBackend.Domain.IRepositories;

namespace TobacoBackend.Services
{
    public class TokenCleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public TokenCleanupService(ILogger<TokenCleanupService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Primera ejecución: 5 minutos después del arranque para no solapar con el inicio.
            // Luego cada 24 horas.
            _timer = new Timer(DoCleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private async void DoCleanup(object? state)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                await repo.CleanExpiredTokensAsync();
                _logger.LogInformation("TokenCleanupService: tokens expirados y revocados eliminados ({Time})", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TokenCleanupService: error durante la limpieza de tokens");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose() => _timer?.Dispose();
    }
}
