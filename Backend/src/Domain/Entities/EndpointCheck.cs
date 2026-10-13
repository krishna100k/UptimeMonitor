using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{

    public class EndpointCheck
    {
        public Guid Id { get; private set; }

        // Foreign key
        public Guid EndpointId { get; private set; }
        // Navigation property
        public Endpoint Endpoint { get; private set; } = null!;
        public long ResponseTimeMs { get; private set; }
        public bool Success { get; private set; }
        public EndpointStatus Status { get; private set; }
        public DateTime CheckedAt { get; private set; }


        // EF Core constructor
        private EndpointCheck()
        {
        }


        public EndpointCheck(
            Guid endpointId,
            long responseTimeMs,
            bool success,
            EndpointStatus status,
            DateTime checkedAt)
        {
            Id = Guid.NewGuid();
            EndpointId = endpointId;
            ResponseTimeMs = responseTimeMs;
            Success = success;
            Status = status;
            CheckedAt = checkedAt;
        }
    }

}
