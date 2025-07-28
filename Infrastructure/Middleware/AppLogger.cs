using Application.Helper;
using Infrastructure.Middleware;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;
        private readonly ICorrelationIdGenerator _correlationId;

        public AppLogger(ILogger<T> logger, ICorrelationIdGenerator correlationId)
        {
            _logger = logger;
            _correlationId = correlationId;
        }

        public virtual void LogInformation(string message)
        {
            _logger.LogInformation($"[CorrelationId: {_correlationId.Get()}] {message}");
        }

        public virtual void LogError(string message)
        {
            _logger.LogError($"[CorrelationId: {_correlationId.Get()}] {message}");
        }

        public virtual void LogWarning(string message)
        {
            _logger.LogWarning($"[CorrelationId: {_correlationId.Get()}] {message}");
        }
    }
}