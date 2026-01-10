namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Base entity class with primary key.
/// All entities should inherit from this or one of its derived classes.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

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

        if (IsTransient() || other.IsTransient())
        {
            return false;
        }

        return Id == other.Id;
    }

    /// <summary>
    /// Gets hash code based on Id.
    /// </summary>
    public override int GetHashCode()
    {
        return IsTransient() ? base.GetHashCode() : Id.GetHashCode();
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

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}
