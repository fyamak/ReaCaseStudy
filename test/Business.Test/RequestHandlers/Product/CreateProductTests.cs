using Business.RequestHandlers.Product;
using Shared.Models.Results;
using Business.Services.Kafka.Interface;
using Moq;
using Autofac;

namespace Business.Test.RequestHandlers.Product;

public class CreateProductTests: BaseHandlerTest
{
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;

    public CreateProductTests()
    {
        _mockKafkaProducer = new Mock<IKafkaProducerService>();
        ContainerBuilder.RegisterInstance(_mockKafkaProducer.Object).As<IKafkaProducerService>();
        
        BuildContainer();
    }

    [Fact]
    public async Task CreateProduct_Success_When_Request_Is_Valid_Test()
    {
        var request = new CreateProduct.CreateProductRequest
        {
            SKU = "TEST-123",
            Name = "Test Product",
            CategoryId = 1
        };

        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<CreateProduct.CreateProductMessage>()))
            .Returns(Task.CompletedTask);

        var response = await Mediator.Send(request);

        Assert.NotNull(response);
        Assert.Equal(ResultStatus.Success, response.Status);

        _mockKafkaProducer.Verify(x =>
            x.ProduceAsync("product-create", It.Is<CreateProduct.CreateProductMessage>(m =>
                m.SKU == request.SKU &&
                m.Name == request.Name &&
                m.CategoryId == request.CategoryId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateProduct_Fail_When_Request_Is_Not_Valid_Test()
    {
        var request1 = new CreateProduct.CreateProductRequest
        {
            Name = "Test Product",
            CategoryId = 1
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        var request2 = new CreateProduct.CreateProductRequest
        {
            SKU = "TEST-123",
            CategoryId = 1
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);

        var request3 = new CreateProduct.CreateProductRequest
        {
            SKU = "TEST-123",
            Name = "Test Product",
            CategoryId = 0 // invalid
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);
    }

    [Fact]
    public async Task CreateProduct_Fail_When_Kafka_Producer_Fails_Test()
    {
        var request = new CreateProduct.CreateProductRequest
        {
            SKU = "TEST-123",
            Name = "Test Product",
            CategoryId = 1
        };

        var expectedException = new Exception("Kafka broker unavailable");
        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<CreateProduct.CreateProductMessage>()))
            .ThrowsAsync(expectedException);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Error, response.Status);
        Assert.Equal(expectedException.Message, response.Message);
    }
}
