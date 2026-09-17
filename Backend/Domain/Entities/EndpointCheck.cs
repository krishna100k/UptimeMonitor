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

        public int? ResponseTimeMs { get; private set; }

        public bool Success { get; private set; }

        public int? StatusCode { get; private set; }

        public DateTimeOffset CheckedAt { get; private set; }


        // EF Core constructor
        private EndpointCheck()
        {
        }


        public EndpointCheck(
            Guid endpointId,
            int? responseTimeMs,
            bool success,
            int? statusCode,
            string? errorMessage,
            DateTimeOffset checkedAt)
        {
            Id = Guid.NewGuid();

            EndpointId = endpointId;
            ResponseTimeMs = responseTimeMs;
            Success = success;
            StatusCode = statusCode;
            CheckedAt = checkedAt;
        }
    }

}
