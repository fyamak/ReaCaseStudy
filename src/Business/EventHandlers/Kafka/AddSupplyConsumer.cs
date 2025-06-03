using Business.Services.Kafka.Interface;
using Confluent.Kafka;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using static Business.RequestHandlers.Product.AddSupply;

namespace Business.EventHandlers.Kafka;

public class AddSupplyConsumer : BackgroundService
{
    private readonly ILogger<AddSupplyConsumer> _logger;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IServiceProvider _serviceProvider;

    public AddSupplyConsumer(
        ILogger<AddSupplyConsumer> logger,
        IKafkaConsumerService kafkaConsumer,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _kafkaConsumer = kafkaConsumer;
        _serviceProvider = serviceProvider;
    }
    public class AddSupplyRequestValidator : AbstractValidator<AddSupplyMessage>
    {
        public AddSupplyRequestValidator()
        {
            RuleFor(x => x.Quantity)
             .GreaterThanOrEqualTo(1)
             .WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product id cannot be empty.");

            RuleFor(x => x.Quantity)
                .NotEmpty()
                .WithMessage("Quantity cannot be empty.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price must be greater than or equal to 0.");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date cannot be empty.");


        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _kafkaConsumer.ConsumeAsync<AddSupplyMessage>(
            "product-add-supply",
            async (message) => await ProcessProductAddSupply(message, stoppingToken),
            stoppingToken);
    }
    private async Task ProcessProductAddSupply(AddSupplyMessage message, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        try
        {
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
            var lockFactory = scope.ServiceProvider.GetService<IDistributedLockFactory>();


            var resource = $"product-supply-lock:{message.ProductId}";
            await using var redLock = await lockFactory.CreateLockAsync(
                resource, 
                TimeSpan.FromSeconds(30), // lock expiration time
                TimeSpan.FromSeconds(5), // wait time
                TimeSpan.FromMilliseconds(500), // retry interval
                cancellationToken);

            if (!redLock.IsAcquired)
            {
                _logger.LogWarning($"Could not acquire lock for product supply: {message.ProductId}");
                return;
            }

            
            var order = await unitOfWork.Orders.GetByIdAsync(message.OrderId);
            if (order == null)
            {
                // MAIL SECTION
                _logger.LogWarning($"Order {message.OrderId} is already processed");
                return;
            }
            order.IsDeleted = true;


            var validator = new AddSupplyRequestValidator();
            var validationResult = validator.Validate(message);

            if (!validationResult.IsValid)
            {
                // MAIL 
                var validationErrorMessage = validationResult.Errors.First().ErrorMessage;
                _logger.LogWarning(validationErrorMessage);
                await FailOrderAsync(order, $"Credentials are invalid. {validationErrorMessage}", unitOfWork);
                return;
            }

            
            var product = await unitOfWork.Products.GetByIdAsync(message.ProductId);
            if (product == null)
            {
                // MAIL SECTION
                _logger.LogWarning("Specified product is not found");
                await FailOrderAsync(order, "Selected product is not in stock", unitOfWork);
                return;
            }
           
            
            var productSupply = new ProductSupply
            {
                ProductId = message.ProductId,
                OrganizationId = message.OrganizationId,
                Quantity = message.Quantity,
                Price = message.Price,
                Date = message.Date,
                RemainingQuantity = message.Quantity
            };
            product.TotalQuantity += message.Quantity;


            order.IsSuccessfull = true;
            order.Detail = "Product supply is successfull";
            order.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Orders.Update(order);
            await unitOfWork.ProductSupplies.AddAsync(productSupply);
            await unitOfWork.Products.Update(product);
            await unitOfWork.CommitAsync();

            // MAIL SECTION
            _logger.LogInformation("Product supply addition is successfull");
            return;
        }
        catch (Exception ex)
        {
            // MAIL SECTION
            _logger.LogError(ex, "Error processing product suplly addition", message.ProductId);
            return;
        }
        finally
        {
            _logger.LogDebug($"Releasing product-supply-lock:{message.ProductId}");
            scope.Dispose();
        }
        
    }

    private async Task FailOrderAsync(Order order, string detail, IUnitOfWork unitOfWork)
    {
        order.IsDeleted = true;
        order.IsSuccessfull = false;
        order.Detail = detail;
        order.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Orders.Update(order);
        await unitOfWork.CommitAsync();
    }
}
