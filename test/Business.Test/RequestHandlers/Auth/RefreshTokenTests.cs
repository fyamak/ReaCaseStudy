using Business.RequestHandlers.Auth;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Auth;

public class RefreshTokenTests : BaseHandlerTest
{
    public RefreshTokenTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task RefreshToken_Success_Test()
    {
        var user = new Infrastructure.Data.Postgres.Entities.User()
        {
            Email        = "test@mail.com",
            FullName     = "Test",
            PhoneNumber = "(555) 555-5555",
            Currency = "$",
            ReceiveEmail = false,
            ReceiveLowStockAlert = false,
            PasswordHash = [1, 2, 3],
            PasswordSalt = [1, 2, 3],
            IsDeleted    = false
        };
        await PostgresContext.Users.AddAsync(user);
        await PostgresContext.SaveChangesAsync();

        var refreshToken = new UserToken(TokenType.RefreshToken, DateTime.UtcNow.AddDays(1), user.Id);
        await PostgresContext.UserTokens.AddAsync(refreshToken);
        await PostgresContext.SaveChangesAsync();

        var request  = new RefreshToken.RefreshTokenRequest() { RefreshToken = refreshToken.Token };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response);
        Assert.NotNull(response.Data!.RefreshToken);
        Assert.NotNull(response.Data.AccessToken);

        var token = await PostgresContext.UserTokens.FirstOrDefaultAsync(u => u.Token == refreshToken.Token);
        Assert.Null(token);
    }

    [Fact]
    public async Task RefreshToken_Fail_When_Token_NotFound_Test()
    {
        var request  = new RefreshToken.RefreshTokenRequest() { RefreshToken = Guid.NewGuid().ToString() };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Token is not valid", response.Message);
    }


    [Fact]
    public async Task RefreshToken_Fail_When_User_Is_Deleted_Test()
    {
        var user = new Infrastructure.Data.Postgres.Entities.User()
        {
            Email        = "test@mail.com",
            FullName     = "Test",
            PhoneNumber = "(555) 555-5555",
            Currency = "$",
            ReceiveEmail = false,
            ReceiveLowStockAlert = false,
            PasswordHash = [1, 2, 3],
            PasswordSalt = [1, 2, 3],
            IsDeleted    = true
        };
        await PostgresContext.Users.AddAsync(user);
        await PostgresContext.SaveChangesAsync();

        var refreshToken = new UserToken(TokenType.RefreshToken, DateTime.UtcNow.AddDays(1), user.Id);
        await PostgresContext.UserTokens.AddAsync(refreshToken);
        await PostgresContext.SaveChangesAsync();

        var request  = new RefreshToken.RefreshTokenRequest() { RefreshToken = Guid.NewGuid().ToString() };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Token is not valid", response.Message);
    }
}
