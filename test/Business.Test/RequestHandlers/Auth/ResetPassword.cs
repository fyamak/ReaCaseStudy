using Autofac;
using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Mail.Interface;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Auth;

public class ResetPassword : BaseHandlerTest
{
    private readonly Mock<IMailService> _mockMailService;

    public ResetPassword()
    {
        _mockMailService = new Mock<IMailService>();

        ContainerBuilder.RegisterInstance(_mockMailService.Object).As<IMailService>();

        BuildContainer();
    }

    [Fact]
    public async Task ResetPassword_Success_Test()
    {
        var user = new Infrastructure.Data.Postgres.Entities.User()
        {
            Email        = "test@email.com",
            FullName     = "Test",
            PasswordHash = [1, 2, 3],
            PasswordSalt = [1, 2, 3],
            IsDeleted    = false
        };

        await PostgresContext.Users.AddAsync(user);
        await PostgresContext.SaveChangesAsync();

        _mockMailService.Setup(m => m.SendMailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var request  = new Business.RequestHandlers.Auth.ResetPassword.ResetPasswordRequest() { Email = user.Email };
        var response = await Mediator.Send(request);

        Assert.NotNull(response);
        Assert.Equal(ResultStatus.Success, response.Status);

        var tokenList = await PostgresContext.UserTokens.Where(t => t.UserId == user.Id && t.TokenType == TokenType.ResetPassword).ToListAsync();

        Assert.NotEmpty(tokenList);
        Assert.Single(tokenList);
    }

    [Fact]
    public async Task ResetPassword_Fail_When_UserNotFound_Test()
    {
        var request  = new Business.RequestHandlers.Auth.ResetPassword.ResetPasswordRequest() { Email = "notfound@email.com" };
        var response = await Mediator.Send(request);

        Assert.NotNull(response);
        Assert.Equal(ResultStatus.Invalid,                              response.Status);
        Assert.Equal($"User with email {request.Email} does not exist", response.Message);
    }

    [Fact]
    public async Task ResetPassword_Fail_When_CantSendEmail_Test()
    {
        _mockMailService.Setup(m => m.SendMailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Cant send email something went wrong"));

        var user = new Infrastructure.Data.Postgres.Entities.User()
        {
            Email        = "test1@email.com",
            FullName     = "Test",
            PasswordHash = [1, 2, 3],
            PasswordSalt = [1, 2, 3],
            IsDeleted    = false
        };

        await PostgresContext.Users.AddAsync(user);
        await PostgresContext.SaveChangesAsync();

        var request  = new Business.RequestHandlers.Auth.ResetPassword.ResetPasswordRequest() { Email = user.Email };
        var response = await Mediator.Send(request);

        Assert.NotNull(response);
        Assert.Equal(ResultStatus.Error,                     response.Status);
        Assert.Equal("Cant send email something went wrong", response.Message);
    }

    [Fact]
    public async Task ResetPassword_Fail_When_EmailIsEmpty_Test()
    {
        var request  = new Business.RequestHandlers.Auth.ResetPassword.ResetPasswordRequest() { Email = string.Empty };
        var response = await Mediator.Send(request);

        Assert.NotNull(response);
        Assert.Equal(ResultStatus.Invalid, response.Status);
    }
}
