using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Models.EndpointRepository
{
    public record EndpointDetails(
        Guid Id,
        string Name,
        string Url,
        EndpointStatus CurrentEndpointStatus,
        double LastMonthUptimePercentage,
        double AverageLatency,
        long P95Latency,
        int ChecksToday,
        List<ResponseTime> LastDayResponseTime,
        List<UptimeHistory> Last90DaysUptime
        );
}
