using Adorika.Application.Common.Wrapper;

using FluentValidation;

using Mediator;

namespace Adorika.Application.Common.Behaviour;

public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken ct)
    {
        // 1. Fast Path: Check for validators without LINQ
        if (validators is not IList<IValidator<TRequest>> list || list.Count == 0)
        {
            return await next(message, ct);
        }

        // 2. Optimized Validation Loop
        List<ResultError>? errors = null;
        var context = new ValidationContext<TRequest>(message);

        for (var i = 0; i < list.Count; i++)
        {
            var result = await list[i].ValidateAsync(context, ct);
            if (!result.IsValid)
            {
                errors ??= new List<ResultError>(result.Errors.Count);
                for (var j = 0; j < result.Errors.Count; j++)
                {
                    var f = result.Errors[j];
                    errors.Add(new ValidationError(f.PropertyName, f.ErrorMessage, f.PropertyName, f.AttemptedValue));
                }
            }
        }

        if (errors == null)
        {
            return await next(message, ct);
        }

        // 3. Result Creation (Merged Logic)
        if (ValidationResultCache.ResultCreator != null)
        {
            var summary = errors.Count == 1 ? "Validation failed: 1 error found" : $"Validation failed: {errors.Count} errors found";
            return (TResponse)ValidationResultCache.ResultCreator(errors, summary);
        }

        throw new ValidationException("Validation failed");
    }

    // Merged Cache & Factory into a private nested class
    private static class ValidationResultCache
    {
        public static readonly Func<IEnumerable<ResultError>, string, object>? ResultCreator;

        static ValidationResultCache()
        {
            var type = typeof(TResponse);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var innerType = type.GetGenericArguments()[0];
                var method = typeof(ValidationResultCache)
                    .GetMethod(nameof(CreateResult), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                    .MakeGenericMethod(innerType);

                if (method != null)
                {
                    ResultCreator = (Func<IEnumerable<ResultError>, string, object>)Delegate.CreateDelegate(typeof(Func<IEnumerable<ResultError>, string, object>), method);
                }
            }
        }

        private static object CreateResult<T>(IEnumerable<ResultError> errors, string message)
            => Result<T>.Failure(errors).WithMessage(message);
    }
}
