using Application.Repositories.Models.EndpointRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.GetEndpoint
{
    public record GetEndpointResponse(EndpointDetails endpointDetails);
}
