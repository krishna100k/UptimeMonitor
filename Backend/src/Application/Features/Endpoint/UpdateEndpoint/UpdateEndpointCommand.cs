using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.UpdateEndpoint
{
    public sealed record UpdateEndpointCommand(Domain.Entities.Endpoint endpoint) : IRequest<UpdateEndpointResponse>;
}
