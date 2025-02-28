using Infrastructure.Data.Postgres.Entities.Base.Interface;

namespace Infrastructure.Data.Postgres.Entities.Base;

public class TrackedBaseEntity<T> : BaseEntity<T>, ITrackedEntity
{
    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool      IsDeleted { get; set; }

    protected TrackedBaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}
