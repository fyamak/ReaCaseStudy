﻿namespace Infrastructure.Data.Postgres.Entities.Base.Interface;

public interface ITrackedEntity
{
    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool      IsDeleted { get; set; }
}
