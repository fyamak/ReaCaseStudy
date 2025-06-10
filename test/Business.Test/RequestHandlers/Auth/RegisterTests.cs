using Business.RequestHandlers.Auth;
using Infrastructure.Data.Postgres.Entities;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Auth;

public class RegisterTests : BaseHandlerTest
{
    public RegisterTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task Register_Fail_When_Request_Is_Not_Valid_Test()
    {
        var request  = new Register.RegisterRequest { Email = "test@test.com", Password = "test" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        request  = new Register.RegisterRequest { Email = "test@test.com", Password = "test", FullName = "1231" };
        response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task Register_Success_When_Request_Is_Valid_Test()
    {
        var request = new Register.RegisterRequest
        {
            Email = "test@test.com",
            Password = "test1234",
            FullName = "Test User",
            PhoneNumber = "(555) 555-5555"
        };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);

        var registeredUser = PostgresContext.Users.First(u => u.Email == request.Email);
        Assert.Equal(registeredUser.FullName, request.FullName);
    }

    [Fact]
    public async Task Register_Fail_When_User_With_Same_Email_Already_Exists_Test()
    {
        var user = new Infrastructure.Data.Postgres.Entities.User()
        {
            Email        = "test@mail.com",
            FullName     = "test test",
            PhoneNumber = "(555) 555-5555",
            Currency = "$",
            ReceiveEmail = false,
            ReceiveLowStockAlert = false,
            PasswordSalt = [1, 2, 3],
            PasswordHash = [1, 2, 3],
            UserType     = UserType.User,
        };

        await PostgresContext.Users.AddAsync(user);
        await PostgresContext.SaveChangesAsync();

        var request  = new Register.RegisterRequest { Email = "test@mail.com", Password = "test_1456325", FullName = "test test", PhoneNumber = "(555) 555-5555" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid,                  response.Status);
        Assert.Equal("User with same email already exists", response.Message);
        Assert.Null(response.Data);
    }
}
