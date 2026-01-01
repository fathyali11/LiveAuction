using System.Linq.Expressions;

namespace LiveAuction.Application.Services.BackgroundJobServices;

public interface IBackgroundJobService
{
    string EnqueueJob<T>(Expression<Action<T>> methodCall);
    string ScheduleJob<T>(Expression<Action<T>> methodCall, TimeSpan delay);
    void AddOrUpdateRecurringJob<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
    void DeleteScheduledJob(string jobId);
}