using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.DeleteEndpoint
{
    public sealed record DeleteEndpointCommand(Guid EndpointId) : IRequest<DeleteEndpointResponse>;
}
