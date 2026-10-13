using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Models.EndpointRepository
{
    public record AllEndpointsStatus(int operational, int degraded, int down);
    public record AllEndpointsList(List<Endpoint> endpoints, AllEndpointsStatus endpointStatus);
}
