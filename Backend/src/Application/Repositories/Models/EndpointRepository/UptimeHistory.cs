using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Models.EndpointRepository
{
    public record UptimeHistory(long ResponseTimeMs, EndpointStatus Status, DateTimeOffset Date);
}
