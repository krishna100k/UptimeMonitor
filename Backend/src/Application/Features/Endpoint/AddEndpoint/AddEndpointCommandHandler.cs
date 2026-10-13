using Application.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.AddEndpoint
{
    public class AddEndpointCommandHandler : IRequestHandler<AddEndpointCommand, AddEndpointResponse>
    {
        private readonly IEndpointRepository _endpointRepository;

        public AddEndpointCommandHandler(IEndpointRepository endpointRepository)
        {
            _endpointRepository = endpointRepository;
        }

        public async Task<AddEndpointResponse> Handle(AddEndpointCommand request, CancellationToken cancellationToken)
        {
            Domain.Entities.Endpoint endPoint = new Domain.Entities.Endpoint(request.Name, request.Url);
            await _endpointRepository.AddEndpoint(endPoint, cancellationToken);
            return new AddEndpointResponse("Endpoint added successfully.");
        }
    }
}
