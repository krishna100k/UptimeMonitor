using Domain.Entities;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class EndpointRepository : IEndpointRepository
    {
        private readonly UptimeDbContext _dbContext;

        public EndpointRepository(UptimeDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddEndpoint(Endpoint endpoint, CancellationToken cancellationToken)
        {
            await _dbContext.AddAsync(endpoint, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
