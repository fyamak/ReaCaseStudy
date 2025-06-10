using Autofac;
using Business.RequestHandlers.Product;
using Moq;
using Shared.Models.Results;
using Business.Services.Kafka.Interface;

namespace Business.Test.RequestHandlers.Product;

public class AddSalesTests : BaseHandlerTest
{
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;

    public AddSalesTests()
    {
        _mockKafkaProducer = new Mock<IKafkaProducerService>();

        ContainerBuilder.RegisterInstance(_mockKafkaProducer.Object).As<IKafkaProducerService>();

        BuildContainer();
    }

    [Fact]
    public async Task AddSales_Success_When_Request_Is_Valid_Test()
    {
        var request = new AddSales.AddSalesRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 1,
            Price = 1.1,
            Date = DateTime.UtcNow,
            OrderId = 1
        };

        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<AddSales.AddSaleMessage>()))
            .Returns(Task.CompletedTask);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal("Adding sale to product request accepted", response.Data);

        _mockKafkaProducer.Verify(x =>
            x.ProduceAsync("product-add-sale", It.Is<AddSales.AddSaleMessage>(m =>
                m.ProductId == request.ProductId &&
                m.OrganizationId == request.OrganizationId &&
                m.Quantity == request.Quantity &&
                m.Price == request.Price &&
                m.Date == request.Date &&
                m.OrderId == request.OrderId)),
            Times.Once);
    }

    [Fact]
    public async Task AddSales_Fail_When_Request_Is_Not_Valid_Test()
    {
        // Missing ProductId
        var request1 = new AddSales.AddSalesRequest
        {
            OrganizationId = 1,
            Quantity = 1,
            Price = 1.1,
            Date = DateTime.UtcNow,
            OrderId = 1
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        // Invalid Quantity
        var request2 = new AddSales.AddSalesRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 0,
            Price = 1.1,
            Date = DateTime.UtcNow,
            OrderId = 1
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);

        // Invalid Price
        var request3 = new AddSales.AddSalesRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 1,
            Price = -1,
            Date = DateTime.UtcNow,
            OrderId = 1
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);
    }

    [Fact]
    public async Task AddSales_Fail_When_Kafka_Producer_Fails_Test()
    {
        var request = new AddSales.AddSalesRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 1,
            Price = 1.1,
            Date = DateTime.UtcNow,
            OrderId = 1
        };

        var expectedException = new Exception("Kafka broker unavailable");
        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<AddSales.AddSaleMessage>()))
            .ThrowsAsync(expectedException);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Error, response.Status);
        Assert.Equal(expectedException.Message, response.Message);
    }
}
