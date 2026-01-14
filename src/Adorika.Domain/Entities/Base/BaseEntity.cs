using Adorika.Domain.Events;

namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Base entity class with primary key.
/// All entities should inherit from this or one of its derived classes.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Checks if this is a transient entity (not yet persisted to database).
    /// </summary>
    public bool IsTransient() => Id == Guid.Empty;

    /// <summary>
    /// Checks entity equality based on Id.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return IsTransient() || other.IsTransient() ? false : Id == other.Id;
    }

    /// <summary>
    /// Gets hash code based on Id.
    /// </summary>
    public override int GetHashCode()
    {
        return IsTransient() ? base.GetHashCode() : Id.GetHashCode();
    }

    /// <summary>
    /// Adds a domain event to the entity's event collection.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event from the entity's event collection.
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the entity.
    /// Typically called after events have been dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        return left is null || right is null ? false : left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}
