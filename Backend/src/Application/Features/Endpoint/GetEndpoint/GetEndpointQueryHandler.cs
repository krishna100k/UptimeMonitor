using Application.Repositories;
using Application.Repositories.Models.EndpointRepository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.GetEndpoint
{
    public class GetEndpointQueryHandler : IRequestHandler<GetEndpointQuery, GetEndpointResponse>
    {
        private readonly IEndpointRepository _endpointRepository;

        public GetEndpointQueryHandler(IEndpointRepository endpointRepository)
        {
            _endpointRepository = endpointRepository;
        }

        public async Task<GetEndpointResponse> Handle(GetEndpointQuery query, CancellationToken cancellationToken)
        {
            EndpointDetails endpointDetails = await _endpointRepository.GetEndpointDetails(query.EndpointId, cancellationToken);
            return new GetEndpointResponse(endpointDetails);
        }
    }
}
