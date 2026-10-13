using Application.Repositories;
using Application.Repositories.Models.EndpointRepository;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundServices
{
    public class EndpointCheckBackgroundService : BackgroundService
    {
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
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]),
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = await factory.CreateConnectionAsync(
            cancellationToken);

            _channel = await _connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: "endpoint-checks",
                durable: true,
                exclusive: false,
                autoDelete: false,
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
           queue: "endpoint-checks",
           autoAck: false,
           consumer: consumer,
           cancellationToken: cancellationToken);

            _logger.LogInformation(
            "EndpointCheckProcessor started. Listening on endpoint-checks queue.");

            // Keep the BackgroundService alive until shutdown.
            try
            {
                await Task.Delay(
                    Timeout.Infinite,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                // Normal application shutdown.
            }

        }

        private async Task ProcessMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message =
                    JsonSerializer.Deserialize<EndpointCheckResult>(
                        json);
                if (message != null)
                {
                    using var scope = _scopeFactory.CreateScope();

                    var endpointRepository = scope.ServiceProvider
                        .GetRequiredService<IEndpointRepository>();

                    await endpointRepository.AddEndpointCheckResults(message, cancellationToken);
                    _logger.LogInformation(
                    "Endpoint check {EndpointId} processed successfully.", message.EndpointId);
                    await _channel!.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken);
                }
                else
                {
                    await _channel!.BasicNackAsync(
                   args.DeliveryTag,
                   multiple: false,
                   requeue: false,
                   cancellationToken);
                    _logger.LogInformation("The endpoint check results are null!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process endpoint check message.");

                await _channel!.BasicNackAsync(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken);
            }


        }

        public override async Task StopAsync(
    CancellationToken cancellationToken)
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
