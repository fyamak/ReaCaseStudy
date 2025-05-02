using Business.Services.Kafka.Interface;
using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Business.RequestHandlers.Order.CreateOrder;

namespace Business.EventHandlers.Kafka;

public class CreateOrderConsumer : BackgroundService
{
    private readonly ILogger<CreateOrderConsumer> _logger;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IServiceProvider _serviceProvider;
    public CreateOrderConsumer(ILogger<CreateOrderConsumer> logger, IKafkaConsumerService kafkaConsumer, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _kafkaConsumer = kafkaConsumer;
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _kafkaConsumer.ConsumeAsync<CreateOrderMessage>(
            "order-create",
            async (message) => await ProcessOrderCreation(message, stoppingToken),
            stoppingToken);
    }

    private async Task ProcessOrderCreation(CreateOrderMessage message, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        try
        {
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();


            var order = new Order
            {
                ProductId = message.ProductId,
                OrganizationId = message.OrganizationId,
                Quantity = message.Quantity,
                Price = message.Price,
                Date = message.Date,
                Type = message.Type
            };
            await unitOfWork.Orders.AddAsync(order);
            await unitOfWork.CommitAsync();

            // MAIL SECTION
            _logger.LogInformation("Organization successfully created", order);
            return;
        }
        catch (Exception ex)
        {
            // MAIL SECTION
            _logger.LogError(ex, "Error processing order creation");
            return;
        }
        finally
        {
            scope.Dispose();
        }

    }
}
