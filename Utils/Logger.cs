using Microsoft.Extensions.Logging;

namespace Test.Utils
{
    public class Logger
    {
        private readonly ILogger<Logger> _logger;

        public Logger(ILogger<Logger> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.LogError(ex, message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }
    }
}