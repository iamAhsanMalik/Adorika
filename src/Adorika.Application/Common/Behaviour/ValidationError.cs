using Adorika.Application.Common.Wrapper;

namespace Adorika.Application.Common.Behaviour;

public sealed record ValidationError : ResultError
{
    public string Field { get; init; }
    public object? AttemptedValue { get; init; }

    public ValidationError(string code, string message, string field, object? attemptedValue)
        : base($"{code}.VALIDATION_ERROR", message, 400)
    {
        Field = field;
        AttemptedValue = attemptedValue;
    }
}
