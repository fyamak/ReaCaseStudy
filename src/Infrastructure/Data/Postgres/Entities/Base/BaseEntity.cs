using Shared.Models.Event;

namespace Infrastructure.Data.Postgres.Entities.Base;

public abstract class BaseEntity<T> : EntityWithEvents
{
    public T Id { get; set; } = default!;
}
