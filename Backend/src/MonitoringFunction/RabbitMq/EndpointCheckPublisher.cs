using Domain.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MonitoringFunction.RabbitMq
{
    public class EndpointCheckPublisher : IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EndpointCheckPublisher> _logger;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private IConnection? _connection;

        public EndpointCheckPublisher(IConfiguration configuration, ILogger<EndpointCheckPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task PublishAsync(EndpointCheckResult result, CancellationToken cancellationToken)
        {
            var connection = await GetOpenConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

            var properties = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: RabbitMqQueues.EndpointChecks,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Endpoint check for {EndpointId} published.", result.EndpointId);
        }

        private async Task<IConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true }) return _connection;

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (_connection is { IsOpen: true }) return _connection;

                if (_connection is not null) await _connection.DisposeAsync();

                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"]!,
                    Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                    UserName = _configuration["RabbitMQ:Username"]!,
                    Password = _configuration["RabbitMQ:Password"]!
                };

                var connection = await factory.CreateConnectionAsync(cancellationToken);
                await DeclareQueuesAsync(connection, cancellationToken);

                _connection = connection;
                return _connection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private static async Task DeclareQueuesAsync(IConnection connection, CancellationToken cancellationToken)
        {
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: RabbitMqQueues.EndpointChecksDeadLetter,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: RabbitMqQueues.EndpointChecks,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: RabbitMqQueues.EndpointChecksArguments(),
                cancellationToken: cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null) await _connection.DisposeAsync();
            _connectionLock.Dispose();
        }
    }
}
