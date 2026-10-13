using System.Text.Json;

namespace Infrastructure.BackgroundServices
{
    public static class MessageFailurePolicy
    {
        public static bool ShouldRetry(Exception exception, bool alreadyRetriedOnce)
        {
            if (alreadyRetriedOnce) return false;

            if (exception is JsonException) return false;

            return true;
        }
    }
}
