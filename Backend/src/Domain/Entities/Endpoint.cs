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

        public DateTime? LastCheckedAt { get; private set; }
        public bool IsPaused { get; private set; } = false;

        public DateTime CreatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

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

            CreatedAt = DateTime.UtcNow;
        }


        public void Update(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

    }

}
