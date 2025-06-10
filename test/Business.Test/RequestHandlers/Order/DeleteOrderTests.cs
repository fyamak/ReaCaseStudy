using Business.RequestHandlers.Order;
using Infrastructure.Data.Postgres.Entities;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Order;

public class DeleteOrderTests : BaseHandlerTest
{
    public DeleteOrderTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task DeleteOrder_Success_When_Order_Exists_Test()
    {
        var order = new Infrastructure.Data.Postgres.Entities.Order
        {
            Id = 1,
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 1,
            Price = 1,
            Date = DateTime.UtcNow,
            Type = "supply"
        };

        await PostgresContext.Orders.AddAsync(order);
        await PostgresContext.SaveChangesAsync();

        var request = new DeleteOrder.DeleteOrderRequest { Id = 1 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal($"Order {order.Id} is successfully deleted.", response.Data);
    }

    [Fact]
    public async Task DeleteOrder_Fail_When_Order_Not_Found_Test()
    {
        var request = new DeleteOrder.DeleteOrderRequest { Id = 9};

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Invalid order Id", response.Message);
    }

    [Fact]
    public async Task DeleteOrder_Fail_When_Order_Already_Deleted_Test()
    {
        var order = new Infrastructure.Data.Postgres.Entities.Order
        {
            Id = 2,
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 1,
            Price = 1,
            Date = DateTime.UtcNow,
            Type = "supply",
            IsDeleted = true,
            IsSuccessfull = false,
            Detail = "Previously deleted"
        };

        await PostgresContext.Orders.AddAsync(order);
        await PostgresContext.SaveChangesAsync();

        var request = new DeleteOrder.DeleteOrderRequest { Id = 2 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Invalid order Id", response.Message);
    }
}
