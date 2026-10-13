using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{

    public class Incident
    {
        public Guid Id { get; private set; }

        // Foreign key
        public Guid EndpointId { get; private set; }

        // Navigation property
        public Endpoint Endpoint { get; private set; } = null!;

        public string? Note { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }


        // EF Core constructor
        private Incident()
        {
        }


        public Incident(
            Guid endpointId,
            DateTimeOffset startedAt,
            string? note = null)
        {
            Id = Guid.NewGuid();

            EndpointId = endpointId;
            Note = note;

            CreatedAt = DateTimeOffset.UtcNow;
        }
    }

}
