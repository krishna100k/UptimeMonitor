using Application.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.GetAllEndpoints
{
    public class GetAllEndpointQueryHandler : IRequestHandler<GetAllEndpointQuery, GetAllEndpointsResponse>
    {
        private readonly IEndpointRepository _endpointRepository;

        public GetAllEndpointQueryHandler(IEndpointRepository endpointRepository)
        {
            _endpointRepository = endpointRepository;
        }
        public async Task<GetAllEndpointsResponse> Handle(GetAllEndpointQuery query, CancellationToken cancellationToken)
        {
            var allEndpointList = await _endpointRepository.GetAllEndpoints(cancellationToken);
            return new GetAllEndpointsResponse(allEndpointList);
        }
    }
}
