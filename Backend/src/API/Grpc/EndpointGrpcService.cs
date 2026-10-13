using Application.Repositories;
using Grpc.Core;
using UptimeMonitor.Grpc;

namespace API.Grpc
{
    public class EndpointGrpcService : EndpointService.EndpointServiceBase
    {
        private readonly IEndpointRepository _repository;

        public EndpointGrpcService(
            IEndpointRepository repository)
        {
            _repository = repository;
        }

        public override async Task<GetActiveEndpointsResponse> GetActiveEndpoints(
            GetActiveEndpointsRequest request,
            ServerCallContext context)
        {
            var endpointsData =
                await _repository.GetAllEndpoints(
                    context.CancellationToken);

            var endpoints = endpointsData.endpoints.Where(e => e.IsPaused == false).ToList();

            var response = new GetActiveEndpointsResponse();

            foreach (var endpoint in endpoints)
            {
                response.Endpoints.Add(new UptimeMonitor.Grpc.Endpoint
                {
                    Id = endpoint.Id.ToString(),
                    Name = endpoint.Name,
                    Url = endpoint.Url
                });
            }

            return response;
        }
    }
}
