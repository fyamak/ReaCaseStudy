using Autofac;
using Business.RequestHandlers.Order;
using Moq;
using Shared.Models.Results;
using Business.Services.Kafka.Interface;
using Serilog;

namespace Business.Test.RequestHandlers.Order;

public class CreateOrderTests : BaseHandlerTest
{
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;

    public CreateOrderTests()
    {
        _mockKafkaProducer = new Mock<IKafkaProducerService>();

        ContainerBuilder.RegisterInstance(_mockKafkaProducer.Object).As<IKafkaProducerService>();

        BuildContainer();
    }

    [Fact]
    public async Task CreateOrder_Success_When_Request_Is_Valid_Test()
    {
        var request = new CreateOrder.CreateOrderRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 5,
            Price = 99.99,
            Date = DateTime.UtcNow,
            Type = "supply"
        };

        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<CreateOrder.CreateOrderMessage>()))
            .Returns(Task.CompletedTask);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal("Order creation request accepted", response.Data);

        _mockKafkaProducer.Verify(x =>
            x.ProduceAsync("order-create", It.Is<CreateOrder.CreateOrderMessage>(m =>
                m.ProductId == request.ProductId &&
                m.OrganizationId == request.OrganizationId &&
                m.Quantity == request.Quantity &&
                m.Price == request.Price &&
                m.Date == request.Date &&
                m.Type == request.Type)),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrder_Fail_When_Request_Is_Not_Valid_Test()
    {
        // Missing ProductId
        var request1 = new CreateOrder.CreateOrderRequest
        {
            OrganizationId = 1,
            Quantity = 5,
            Price = 99.99,
            Date = DateTime.UtcNow,
            Type = "supply"
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        // Invalid Quantity (0)
        var request2 = new CreateOrder.CreateOrderRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 0,
            Price = 99.99,
            Date = DateTime.UtcNow,
            Type = "supply"
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);

        // Negative Price
        var request3 = new CreateOrder.CreateOrderRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 5,
            Price = -1,
            Date = DateTime.UtcNow,
            Type = "supply"
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);

        // Missing Type
        var request4 = new CreateOrder.CreateOrderRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 5,
            Price = 99.99,
            Date = DateTime.UtcNow
        };
        var response4 = await Mediator.Send(request4);
        Assert.Equal(ResultStatus.Invalid, response4.Status);
    }

    [Fact]
    public async Task CreateOrder_Fail_When_Kafka_Producer_Fails_Test()
    {
        var request = new CreateOrder.CreateOrderRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 5,
            Price = 99.99,
            Date = DateTime.UtcNow,
            Type = "supply"
        };

        var expectedException = new Exception("Kafka broker unavailable");
        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<CreateOrder.CreateOrderMessage>()))
            .ThrowsAsync(expectedException);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Error, response.Status);
        Assert.Equal(expectedException.Message, response.Message);
    }
}
