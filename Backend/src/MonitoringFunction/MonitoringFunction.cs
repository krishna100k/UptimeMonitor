using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Domain.Messages;
using MonitoringFunction.RabbitMq;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using UptimeMonitor.Grpc;

namespace MonitoringFunction
{
    public class MonitoringFunction
    {
        private readonly EndpointService.EndpointServiceClient _client;
        private readonly ILogger<MonitoringFunction> _logger;
        private readonly HttpClient _httpClient;
        private readonly EndpointCheckPublisher _publisher;

        public MonitoringFunction(ILogger<MonitoringFunction> logger, HttpClient httpClient, EndpointService.EndpointServiceClient client, EndpointCheckPublisher publisher)
        {
            _logger = logger;
            _httpClient = httpClient;
            _client = client;
            _publisher = publisher;
        }

        [Function("MonitorEndpoints")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo timer, CancellationToken cancellationToken)
        {
            const int degradedThresholdMs = 1000;
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response =
            await _client.GetActiveEndpointsAsync(
                new GetActiveEndpointsRequest(),
                cancellationToken: cancellationToken);
            var endpoints = response.Endpoints;

            var options = new ParallelOptions { MaxDegreeOfParallelism = 20 };
            await Parallel.ForEachAsync(endpoints, options, async (endpoint, token) =>
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var status = EndpointStatus.Unknown;
                    var response = await _httpClient.GetAsync(
                        endpoint.Url,
                        token);

                    stopwatch.Stop();

                    var responseTimeMs =
                        stopwatch.ElapsedMilliseconds;

                    if (response.IsSuccessStatusCode)
                    {
                        status = EndpointStatus.Up;
                        if (responseTimeMs >= degradedThresholdMs) status = EndpointStatus.Degraded;
                    }
                    else
                    {
                        status = EndpointStatus.Down;
                    }

                    EndpointCheckResult result = new EndpointCheckResult
                    {
                        EndpointId = Guid.Parse(endpoint.Id),
                        Status = status,
                        ResponseTimeMs = responseTimeMs,
                        Success = response.IsSuccessStatusCode,
                        CheckedAt = DateTime.UtcNow,
                    };

                    _logger.LogInformation(
                        "{Url} returned {StatusCode} in {ResponseTimeMs}ms",
                        endpoint.Url,
                        response.StatusCode,
                        responseTimeMs);

                    try
                    {
                        await _publisher.PublishAsync(result, token);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Checked {Url} but failed to publish the result to RabbitMQ", endpoint.Url);
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    _logger.LogError(
                        ex,
                        "Failed to check {Url} after {ResponseTimeMs}ms",
                        endpoint.Url,
                        stopwatch.ElapsedMilliseconds);
                }
            });
        }
    }
}
