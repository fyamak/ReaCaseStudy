using Autofac;
using Business.RequestHandlers.Organization;
using Moq;
using Shared.Models.Results;
using Business.Services.Kafka.Interface;

namespace Business.Test.RequestHandlers.Organization;

public class CreateOrganizationTests : BaseHandlerTest
{
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;

    public CreateOrganizationTests()
    {
        _mockKafkaProducer = new Mock<IKafkaProducerService>();

        ContainerBuilder.RegisterInstance(_mockKafkaProducer.Object).As<IKafkaProducerService>();

        BuildContainer();
    }

    [Fact]
    public async Task CreateOrganization_Success_When_Request_Is_Valid_Test()
    {
        var request = new CreateOrganization.CreateOrganizationRequest
        {
            Name = "Test Org",
            Email = "test@org.com",
            Phone = "1234567890",
            Address = "123 Test St"
        };

        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<CreateOrganization.CreateOrganizationMessage>()))
            .Returns(Task.CompletedTask);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal("Organization creation request accepted", response.Data);

        _mockKafkaProducer.Verify(x =>
            x.ProduceAsync("organization-create", It.Is<CreateOrganization.CreateOrganizationMessage>(m =>
                m.Name == request.Name &&
                m.Email == request.Email &&
                m.Phone == request.Phone &&
                m.Address == request.Address)),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrganization_Fail_When_Request_Is_Not_Valid_Test()
    {
        // Missing Name
        var request1 = new CreateOrganization.CreateOrganizationRequest
        {
            Email = "test@org.com",
            Phone = "1234567890",
            Address = "123 Test St"
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        // Missing Email
        var request2 = new CreateOrganization.CreateOrganizationRequest
        {
            Name = "Test Org",
            Phone = "1234567890",
            Address = "123 Test St"
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);

        // Invalid Email Format
        var request3 = new CreateOrganization.CreateOrganizationRequest
        {
            Name = "Test Org",
            Email = "invalid-email",
            Phone = "1234567890",
            Address = "123 Test St"
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);

        // Missing Phone
        var request4 = new CreateOrganization.CreateOrganizationRequest
        {
            Name = "Test Org",
            Email = "test@org.com",
            Address = "123 Test St"
        };
        var response4 = await Mediator.Send(request4);
        Assert.Equal(ResultStatus.Invalid, response4.Status);
    }

    [Fact]
    public async Task CreateOrganization_Fail_When_Kafka_Producer_Fails_Test()
    {
        var request = new CreateOrganization.CreateOrganizationRequest
        {
            Name = "Test Org",
            Email = "test@org.com",
            Phone = "1234567890",
            Address = "123 Test St"
        };

        var expectedException = new Exception("Kafka broker unavailable");
        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<CreateOrganization.CreateOrganizationMessage>()))
            .ThrowsAsync(expectedException);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Error, response.Status);
        Assert.Equal(expectedException.Message, response.Message);
    }
}
