﻿using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.Repositories;

public class UserTokenRepository : Repository<UserToken, string>, IUserTokenRepository
{
    public UserTokenRepository(PostgresContext postgresContext) : base(postgresContext)
    {
    }

    public async Task<UserToken?> GetTokenWithUserAsync(string token)
    {
        return await PostgresContext.UserTokens
            .AsNoTracking()
            .Include(u => u.User)
            .FirstOrDefaultAsync(u => u.Token == token);
    }
}
