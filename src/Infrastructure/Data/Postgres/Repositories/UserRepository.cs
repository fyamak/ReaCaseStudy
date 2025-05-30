﻿using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;


namespace Infrastructure.Data.Postgres.Repositories;

public class UserRepository : TrackedEntityRepository<User, int>, IUserRepository
{
    public UserRepository(PostgresContext postgresContext) : base(postgresContext)
    {
    }

    public async Task<int> Update(User user)
    {
        PostgresContext.Users.Update(user);
        return await PostgresContext.SaveChangesAsync();
    }
}
