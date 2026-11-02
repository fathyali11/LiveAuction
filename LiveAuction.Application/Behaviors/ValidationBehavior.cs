using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Application.Behaviors;

/// <summary>
/// Runs every <see cref="IValidator{T}"/> registered for a request before its handler executes.
/// A failed validation throws <see cref="ValidationException"/>, which the API's global
/// exception handler converts into a 400 response.
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> _validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> _logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        _logger.LogWarning(
            "Validation failed for {RequestName}: {Errors}",
            typeof(TRequest).Name,
            string.Join("; ", failures.Select(failure => failure.ErrorMessage)));

        throw new ValidationException(failures);
    }
}
