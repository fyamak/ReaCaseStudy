﻿using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface;

public interface IUserRepository : ITrackedEntityRepository<User, int>
{
    public Task<int> Update(User user);
}
