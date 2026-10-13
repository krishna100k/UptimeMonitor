using Application.Repositories;
using Domain.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Infrastructure.BackgroundServices
{
    public class EndpointCheckBackgroundService : BackgroundService
    {
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ILogger<EndpointCheckBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public EndpointCheckBackgroundService(
        ILogger<EndpointCheckBackgroundService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await StartConsumingWithRetryAsync(cancellationToken);

                // for keeping the BackgroundService alive until shutdown.
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Normal application shutdown.
            }
        }

        private async Task StartConsumingWithRetryAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    await StartConsumingAsync(cancellationToken);
                    return;
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        ex,
                        "Could not connect to RabbitMQ. Retrying in {Seconds} seconds.",
                        ReconnectDelay.TotalSeconds);

                    await CloseConnectionAsync(CancellationToken.None);
                    await Task.Delay(ReconnectDelay, cancellationToken);
                }
            }
        }

        private async Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"]!,
                Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                UserName = _configuration["RabbitMQ:Username"]!,
                Password = _configuration["RabbitMQ:Password"]!,
                ConsumerDispatchConcurrency = 10
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: RabbitMqQueues.EndpointChecksDeadLetter,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: RabbitMqQueues.EndpointChecks,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: RabbitMqQueues.EndpointChecksArguments(),
                cancellationToken: cancellationToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 10,
                global: false,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                await ProcessMessageAsync(args, cancellationToken);
            };

            await _channel.BasicConsumeAsync(
                queue: RabbitMqQueues.EndpointChecks,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "EndpointCheckProcessor started. Listening on {Queue} queue.", RabbitMqQueues.EndpointChecks);
        }

        private async Task ProcessMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<EndpointCheckResult>(json);

                if (message == null)
                {
                    _logger.LogWarning("The endpoint check message was null. Sending it to the dead-letter queue.");
                    await RejectAsync(args, cancellationToken);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var endpointRepository = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();

                await endpointRepository.AddEndpointCheckResults(message, cancellationToken);

                await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
                _logger.LogInformation("Endpoint check {EndpointId} processed successfully.", message.EndpointId);
            }
            catch (Exception ex)
            {
                var retry = MessageFailurePolicy.ShouldRetry(ex, alreadyRetriedOnce: args.Redelivered);

                _logger.LogError(
                    ex,
                    "Failed to process endpoint check message. {Outcome}",
                    retry ? "Retrying once." : "Sending it to the dead-letter queue.");

                await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: retry, cancellationToken);
            }
        }

        private Task RejectAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            return _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken).AsTask();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await CloseConnectionAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }

        private async Task CloseConnectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_channel is not null)
                {
                    await _channel.CloseAsync(cancellationToken);
                    await _channel.DisposeAsync();
                    _channel = null;
                }

                if (_connection is not null)
                {
                    await _connection.CloseAsync(cancellationToken);
                    await _connection.DisposeAsync();
                    _connection = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ignoring error while closing the RabbitMQ connection.");
            }
        }
    }
}
