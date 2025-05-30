﻿namespace Business.Services.Security.Auth.UserPassword.Interface;

public interface IUserPasswordHashingService
{
    void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    bool VerifyPasswordHash(string password, byte[]     passwordHash, byte[]     passwordSalt);
}
