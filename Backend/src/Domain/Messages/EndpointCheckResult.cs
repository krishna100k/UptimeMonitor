using Domain.Enums;

namespace Domain.Messages
{
    // The message sent over RabbitMQ from MonitoringFunction to the API.
    // One shared class means both sides can never drift apart.
    public class EndpointCheckResult
    {
        public Guid EndpointId { get; set; }

        public EndpointStatus Status { get; set; }

        public long ResponseTimeMs { get; set; }

        public bool Success { get; set; }

        public DateTime CheckedAt { get; set; }
    }
}
