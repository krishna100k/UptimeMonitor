using Application.Repositories;
using Application.Repositories.Models;
using Application.Repositories.Models.EndpointRepository;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Application.Exceptions.Exceptions;

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

        public async Task<EndpointDetails> GetEndpointDetails(Guid endpointId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var endpoint = await _dbContext.Endpoint
                .FirstOrDefaultAsync(
                    e => e.Id == endpointId,
                    cancellationToken);

            if (endpoint == null)
            {
                throw new NotFoundException("Endpoint not found.");
            }

            var ninetyDaysAgo = now.AddDays(-90);

            var last90DaysUptime = await _dbContext.EndpointCheck
                .Where(x =>
                    x.EndpointId == endpointId &&
                    x.CheckedAt >= ninetyDaysAgo)
                .Select(x => new UptimeHistory(
                    x.ResponseTimeMs,
                    x.Status,
                    x.CheckedAt))
                .ToListAsync(cancellationToken);

            var averageLatency = await _dbContext.EndpointCheck
                .Where(x =>
                    x.EndpointId == endpointId)
                .Select(x => x.ResponseTimeMs)
                .DefaultIfEmpty()
                .AverageAsync(cancellationToken);

            var startOfToday = now.Date;
            var startOfTomorrow = startOfToday.AddDays(1);

            var checksToday = await _dbContext.EndpointCheck
                .Where(x =>
                    x.EndpointId == endpointId &&
                    x.CheckedAt >= startOfToday &&
                    x.CheckedAt < startOfTomorrow)
                .CountAsync(cancellationToken);


            var last24Hours = now.AddHours(-24);

            var lastDayResponseTime = await _dbContext.EndpointCheck
                .Where(x =>
                    x.EndpointId == endpointId &&
                    x.CheckedAt >= last24Hours)
                .Select(x => new ResponseTime(
                    x.ResponseTimeMs,
                    x.CheckedAt))
                .ToListAsync(cancellationToken);


            var thirtyDaysAgo = now.AddDays(-30);

            var last30DaysChecks = await _dbContext.EndpointCheck
                .Where(x =>
                    x.EndpointId == endpointId &&
                    x.CheckedAt >= thirtyDaysAgo)
                .ToListAsync(cancellationToken);

            var last30DaysSuccessCount = last30DaysChecks
                .Count(x => x.Status == Domain.Enums.EndpointStatus.Up);

            var last30DaysSuccessPercentage =
                last30DaysChecks.Count == 0
                    ? 0
                    : (double)last30DaysSuccessCount
                        / last30DaysChecks.Count
                        * 100;


            var sortedLatencies = last90DaysUptime
                .Select(x => x.ResponseTimeMs)
                .OrderBy(x => x)
                .ToList();

            var p95Latency = sortedLatencies.Count == 0
                ? 0
                : sortedLatencies[
                    (int)Math.Ceiling(
                        sortedLatencies.Count * 0.95) - 1
                ];

            var currentEndpointStatus = await _dbContext.EndpointCheck
            .Where(x => x.EndpointId == endpointId)
            .OrderByDescending(x => x.CheckedAt)
            .Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);

            EndpointDetails details = new EndpointDetails(
                endpointId,
                endpoint.Name,
                endpoint.Url,
                currentEndpointStatus,
                last30DaysSuccessPercentage,
                averageLatency,
                p95Latency,
                checksToday,
                lastDayResponseTime,
                last90DaysUptime
            );

            return details;

        }

        public async Task UpdateEndpoint(Endpoint endpoint, CancellationToken cancellationToken)
        {
            Endpoint? existingEndpoint = await _dbContext.Endpoint.Where(e => e.Id == endpoint.Id && !e.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            if (existingEndpoint == null)
            {
                throw new NotFoundException("Endpoint not found.");
            }

            existingEndpoint.Update(endpoint.Name, endpoint.Url);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteEndpoint(Guid endpointId, CancellationToken cancellationToken)
        {
            Endpoint? endpoint = await _dbContext.Endpoint.Where(e => e.Id == endpointId && !e.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            if(endpoint == null)
            {
                throw new NotFoundException("Endpoint not found.");
            }
            endpoint.Delete();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<AllEndpointsList> GetAllEndpoints(CancellationToken cancellationToken)
        {
            int operational = 0;
            int degraded = 0;
            int down = 0;

            List<Endpoint> endpointsList = await _dbContext.Endpoint.Where(e => e.IsDeleted == false).ToListAsync(cancellationToken);
            var latestChecks = await _dbContext.EndpointCheck
                .GroupBy(x => x.EndpointId)
                .Select(g => g
                    .OrderByDescending(x => x.CheckedAt)
                    .First())
                .ToListAsync(cancellationToken);

            operational = latestChecks.Count(x =>
                x.Status == EndpointStatus.Up);

            degraded = latestChecks.Count(x =>
                x.Status == EndpointStatus.Degraded);

            down = latestChecks.Count(x =>
                x.Status == EndpointStatus.Down);

            var endpointStatuses = new AllEndpointsStatus(operational, degraded, down);
            var allEndpointList = new AllEndpointsList(endpointsList, endpointStatuses);
            return allEndpointList;
        }

        public async Task AddEndpointCheckResults(EndpointCheckResult result, CancellationToken cancellationToken)
        {
            EndpointCheck check = new EndpointCheck(result.EndpointId, result.ResponseTimeMs, result.Success, result.Status, result.CheckedAt);
            await _dbContext.EndpointCheck.AddAsync(check, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

    }
}
