using Hangfire;
using LiveAuction.Domain.Services;
using System.Linq.Expressions;

namespace LiveAuction.Infrastructure.Services;

internal class BackgroundJobService : IBackgroundJobService
{

    public string EnqueueJob<T>(Expression<Action<T>> methodCall)
    {
        return BackgroundJob.Enqueue<T>(methodCall);
    }
    public string ScheduleJob<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule<T>(methodCall, delay);
    }

    public void AddOrUpdateRecurringJob<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate<T>(jobId, methodCall, cronExpression);
    }
}