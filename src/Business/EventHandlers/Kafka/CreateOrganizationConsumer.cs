using Business.Services.Kafka.Interface;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Business.RequestHandlers.Organization.CreateOrganization;

namespace Business.EventHandlers.Kafka;

public class CreateOrganizationConsumer : BackgroundService
{
    private readonly ILogger<CreateOrganizationConsumer> _logger;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IServiceProvider _serviceProvider;

    public CreateOrganizationConsumer(ILogger<CreateOrganizationConsumer> logger, IKafkaConsumerService kafkaConsumer, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _kafkaConsumer = kafkaConsumer;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _kafkaConsumer.ConsumeAsync<CreateOrganizationMessage>(
            "organization-create",
            async (message) => await ProcessOrganizationCreation(message, stoppingToken),
            stoppingToken);
    }

    private async Task ProcessOrganizationCreation(CreateOrganizationMessage message, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        try
        {
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();

            if (await unitOfWork.Organizations.CountAsync(msg => msg.Name == message.Name) > 0)
            {
                // MAIL SECTION
                _logger.LogWarning("Organization with same name already exists");
                return;
            }

            var organization = new Organization 
            {
                Name = message.Name,
                Email = message.Email,
                Phone = message.Phone,
                Address = message.Address

            };
            await unitOfWork.Organizations.AddAsync(organization);
            await unitOfWork.CommitAsync();

            // MAIL SECTION
            _logger.LogInformation("Organization successfully created", organization);
            return;
        }
        catch (Exception ex)
        {
            // MAIL SECTION
            _logger.LogError(ex, "Error processing organization creation", message.Name);
            return;
        }
        finally
        {
            scope.Dispose();
        }
    }

}
