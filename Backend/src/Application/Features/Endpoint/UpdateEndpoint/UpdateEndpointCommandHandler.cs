using Application.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.UpdateEndpoint
{
    public class UpdateEndpointCommandHandler : IRequestHandler<UpdateEndpointCommand, UpdateEndpointResponse>
    {
        private IEndpointRepository _endpointRepository;
        public UpdateEndpointCommandHandler(IEndpointRepository endpointRepository) 
        {
            _endpointRepository = endpointRepository;
        }

        public async Task<UpdateEndpointResponse> Handle(UpdateEndpointCommand command, CancellationToken cancellationToken)
        {
            await _endpointRepository.UpdateEndpoint(command.endpoint, cancellationToken);
            return new UpdateEndpointResponse("Endpoint details updated successfully");
        }
    }
}
