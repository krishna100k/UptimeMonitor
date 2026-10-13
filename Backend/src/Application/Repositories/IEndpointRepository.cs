using Application.Repositories.Models.EndpointRepository;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IEndpointRepository
    {
        Task AddEndpoint(Endpoint endpoint, CancellationToken cancellationToken);
        Task<EndpointDetails> GetEndpointDetails(Guid EndpointId, CancellationToken cancellationToken);
        Task DeleteEndpoint(Guid endpointId, CancellationToken cancellationToken);
        Task UpdateEndpoint(Endpoint endpoint, CancellationToken cancellationToken);
        Task<AllEndpointsList> GetAllEndpoints(CancellationToken cancellationToken);
        Task AddEndpointCheckResults(EndpointCheckResult result, CancellationToken cancellationToken);

    }
}
