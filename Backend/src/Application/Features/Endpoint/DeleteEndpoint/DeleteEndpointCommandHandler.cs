using Application.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.DeleteEndpoint
{
    public class DeleteEndpointCommandHandler : IRequestHandler<DeleteEndpointCommand, DeleteEndpointResponse>
    {
        private IEndpointRepository _endpointRepository;
        public DeleteEndpointCommandHandler(IEndpointRepository endpointRepository)
        {
            _endpointRepository = endpointRepository;
        }

        public async Task<DeleteEndpointResponse> Handle(DeleteEndpointCommand command, CancellationToken cancellationToken) 
        {
           await _endpointRepository.DeleteEndpoint(command.EndpointId, cancellationToken);
            return new DeleteEndpointResponse("Endpoint deleted successfully");
        }
    }
}
