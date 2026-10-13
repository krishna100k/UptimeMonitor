namespace Domain.Messages
{
    // Queue names and settings shared by the publisher and the consumer.
    // Both sides must declare the queue with identical arguments, otherwise RabbitMQ refuses.
    public static class RabbitMqQueues
    {
        public const string EndpointChecks = "endpoint-checks";
        public const string EndpointChecksDeadLetter = "endpoint-checks.dead";

        // Rejected messages (requeue: false) are routed to the dead-letter queue instead of vanishing.
        public static Dictionary<string, object?> EndpointChecksArguments() => new()
        {
            ["x-dead-letter-exchange"] = "",
            ["x-dead-letter-routing-key"] = EndpointChecksDeadLetter
        };
    }
}
