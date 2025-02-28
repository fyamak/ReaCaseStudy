using Autofac;
using Business.RequestHandlers.Auth;
using Business.Services.Security.Auth.UserPassword.Interface;
using Infrastructure.Data.Postgres.Entities;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Auth;

public class LoginTests : BaseHandlerTest
{
    public LoginTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task Login_Fail_When_User_NotFound_Test()
    {
        var request  = new Login.LoginRequest { Email = "test@test.com", Password = "test" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task Login_Fail_When_Request_Is_Not_Valid_Test()
    {
        var request  = new Login.LoginRequest { Email = "test@test.com" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        request  = new Login.LoginRequest { Password = "test" };
        response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task Login_Success_When_User_Found_Test()
    {
        var userPasswordHashingService = Container.Resolve<IUserPasswordHashingService>();
        userPasswordHashingService.CreatePasswordHash("test", out var passwordHash, out var passwordSalt);
        var userToLogin = new Infrastructure.Data.Postgres.Entities.User
        {
            FullName = "test", Email = "test@test.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt
        };
        await PostgresContext.Users.AddAsync(userToLogin);
        await PostgresContext.SaveChangesAsync();

        var request  = new Login.LoginRequest() { Email = "test@test.com", Password = "test" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.AccessToken);

        var refreshTokenOnDb = PostgresContext.UserTokens.First(u => u.UserId == userToLogin.Id && u.TokenType == TokenType.RefreshToken);
        Assert.Equal(refreshTokenOnDb.Token, response.Data.RefreshToken);
    }

    [Fact]
    public async Task Login_Fail_When_User_Deleted_Test()
    {
        var userPasswordHashingService = Container.Resolve<IUserPasswordHashingService>();
        userPasswordHashingService.CreatePasswordHash("test", out var passwordHash, out var passwordSalt);
        var userToLogin = new Infrastructure.Data.Postgres.Entities.User
        {
            FullName     = "test",
            Email        = "test@test.com",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsDeleted    = true
        };
        await PostgresContext.Users.AddAsync(userToLogin);
        await PostgresContext.SaveChangesAsync();

        var request  = new Login.LoginRequest() { Email = "test@test.com", Password = "test" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
    }
}
