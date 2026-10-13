using Grpc.Net.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitoringFunction.RabbitMq;
using UptimeMonitor.Grpc;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var grpcChannel =
    GrpcChannel.ForAddress(
        context.Configuration["GrpcUrl"]!);

        services.AddSingleton(grpcChannel);
        
        services.AddSingleton(
            serviceProvider =>
            {
                var channel =
                    serviceProvider.GetRequiredService<GrpcChannel>();

                return new EndpointService.EndpointServiceClient(channel);
            });
        services.AddSingleton<EndpointCheckPublisher>();
        services.AddHttpClient();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();