using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitoringFunction.Contracts.Messages;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonitoringFunction.RabbitMq
{
    public class EndpointCheckPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EndpointCheckPublisher> _logger;

        public EndpointCheckPublisher(IConfiguration configuration, ILogger<EndpointCheckPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task PublishAsync(EndpointCheckResult result, CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"]!,
                Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                UserName = _configuration["RabbitMQ:Username"]!,
                Password = _configuration["RabbitMQ:Password"]!
            };

            using var connection = await factory.CreateConnectionAsync(cancellationToken);
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue : "endpoint-checks",
                durable : true,
                exclusive : false,
                autoDelete : false
                );

            var json = JsonSerializer.Serialize(result);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange : "",
                routingKey : "endpoint-checks",
                body : body
                );

            _logger.LogInformation("Endpoint published successfully.");
        }
    }
}
