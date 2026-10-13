using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringFunction.Contracts.Messages
{
    public class EndpointCheckResult
    {
        public Guid EndpointId { get; set; }

        public EndpointStatus Status { get; set; }

        public long ResponseTimeMs { get; set; }

        public bool Success { get; set; }

        public DateTime CheckedAt { get; set; }
    }
}
