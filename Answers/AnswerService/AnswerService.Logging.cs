using Microsoft.Extensions.Logging;


namespace Answers
{
    public partial class AnswerService
    {
        public void LogWarning(string message)
        {
            _logger?.LogWarning(message);
        }

        public void LogError(string message)
        {
            _logger?.LogError(message);
        }

        public void LogInfo(string message)
        {
            _logger?.LogInformation(message);
        }
    }
}
