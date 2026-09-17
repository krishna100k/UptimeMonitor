using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    using Domain.Enums;

    public class Endpoint
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; } = null!;

        public string Url { get; private set; } = null!;

        public DateTimeOffset? LastCheckedAt { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        // Navigation properties
        public ICollection<EndpointCheck> Checks { get; private set; }
            = new List<EndpointCheck>();

        public ICollection<Incident> Incidents { get; private set; }
            = new List<Incident>();


        // EF Core constructor
        private Endpoint()
        {
        }


        public Endpoint(string name, string url)
        {
            Id = Guid.NewGuid();

            Name = name;
            Url = url;

            CreatedAt = DateTimeOffset.UtcNow;
        }


        public void Update(string name, string url)
        {
            Name = name;
            Url = url;
        }

    }

}
