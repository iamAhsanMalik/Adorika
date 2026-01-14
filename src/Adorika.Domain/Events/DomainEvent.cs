namespace Adorika.Domain.Events;

/// <summary>
/// Base class for domain events.
/// Provides default implementation for common event properties.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional correlation ID for tracking related events.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Optional causation ID for tracking event chains.
    /// </summary>
    public string? CausationId { get; init; }
}
