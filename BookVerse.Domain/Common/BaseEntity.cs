namespace BookVerse.Domain.Common;
public abstract class BaseEntity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; protected init; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }
}