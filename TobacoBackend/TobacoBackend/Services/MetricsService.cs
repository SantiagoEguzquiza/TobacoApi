using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TobacoBackend.Services
{
    /// <summary>
    /// Servicio para métricas y contadores de performance
    /// </summary>
    public class MetricsService
    {
        private readonly Meter _meter;
        private readonly Counter<long> _requestCounter;
        private readonly Counter<long> _errorCounter;
        private readonly Histogram<double> _requestDuration;
        private readonly Counter<long> _loginAttemptsCounter;
        private readonly Counter<long> _failedLoginAttemptsCounter;

        public MetricsService()
        {
            _meter = new Meter("TobacoBackend", "1.0.0");
            
            _requestCounter = _meter.CreateCounter<long>(
                "tobaco_requests_total",
                "requests",
                "Total number of HTTP requests");
            
            _errorCounter = _meter.CreateCounter<long>(
                "tobaco_errors_total",
                "errors",
                "Total number of HTTP errors");
            
            _requestDuration = _meter.CreateHistogram<double>(
                "tobaco_request_duration_seconds",
                "seconds",
                "HTTP request duration in seconds");
            
            _loginAttemptsCounter = _meter.CreateCounter<long>(
                "tobaco_login_attempts_total",
                "logins",
                "Total number of login attempts");
            
            _failedLoginAttemptsCounter = _meter.CreateCounter<long>(
                "tobaco_failed_login_attempts_total",
                "logins",
                "Total number of failed login attempts");
        }

        public void RecordRequest(string method, string path, int statusCode, double durationSeconds)
        {
            var tags = new TagList
            {
                { "method", method },
                { "path", path },
                { "status_code", statusCode.ToString() }
            };

            _requestCounter.Add(1, tags);
            _requestDuration.Record(durationSeconds, tags);

            if (statusCode >= 400)
            {
                _errorCounter.Add(1, tags);
            }
        }

        public void RecordLoginAttempt(bool success)
        {
            _loginAttemptsCounter.Add(1);
            if (!success)
            {
                _failedLoginAttemptsCounter.Add(1);
            }
        }
    }
}

