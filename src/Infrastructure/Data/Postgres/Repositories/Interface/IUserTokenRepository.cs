﻿using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface;

public interface IUserTokenRepository : IRepository<UserToken, string>
{
    Task<UserToken?> GetTokenWithUserAsync(string token);
}
