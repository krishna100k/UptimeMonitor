using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class UptimeDbContext : DbContext
    {
        public UptimeDbContext(DbContextOptions<UptimeDbContext> options) : base(options) { }

        public DbSet<Endpoint> Endpoint { get; set; }
        public DbSet<EndpointCheck> EndpointCheck { get; set; }
        public DbSet<Incident> Incident { get; set; }
    }
}
