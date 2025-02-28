using Autofac;
using Business.RequestHandlers.User;
using Business.Services.Security.Auth.Jwt.Interface;
using Infrastructure.Data.Postgres.Entities;
using Moq;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.User;

public class GetProfileInfoTests : BaseHandlerTest
{
    private readonly Mock<IUserContext> _mockUserContext;

    public GetProfileInfoTests()
    {
        _mockUserContext = new Mock<IUserContext>();

        ContainerBuilder.RegisterInstance(_mockUserContext.Object).As<IUserContext>();

        BuildContainer();
    }

    [Fact]
    public async Task GetProfileInfo_Success_Test()
    {
        var user = new Infrastructure.Data.Postgres.Entities.User
        {
            Email        = "test@email.com",
            FullName     = "test",
            PasswordHash = [1, 2, 3],
            PasswordSalt = [1, 2, 3],
            UserType     = UserType.User
        };

        await PostgresContext.Users.AddAsync(user);
        await PostgresContext.SaveChangesAsync();

        _mockUserContext.Setup(u => u.GetUserId()).Returns(user.Id);

        var request  = new GetProfileInfo.GetProfileInfoRequest();
        var response = await Mediator.Send(request);

        Assert.NotNull(response.Data);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(user.Email,           response.Data.Email);
        Assert.Equal(user.Id,              response.Data.Id);
        Assert.Equal(user.FullName,        response.Data.FullName);
    }

    [Fact]
    public async Task GetProfileInfo_Fail_When_User_Deleted_Test()
    {
        var user = new Infrastructure.Data.Postgres.Entities.User
        {
            Email        = "testDeleted@email.com",
            FullName     = "test",
            PasswordHash = [1, 2, 3],
            PasswordSalt = [1, 2, 3],
            UserType     = UserType.User,
            IsDeleted    = true
        };

        await PostgresContext.Users.AddAsync(user);
        await PostgresContext.SaveChangesAsync();

        _mockUserContext.Setup(u => u.GetUserId()).Returns(user.Id);

        var request  = new GetProfileInfo.GetProfileInfoRequest();
        var response = await Mediator.Send(request);

        Assert.Null(response.Data);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("User not found",     response.Message);
    }
}
