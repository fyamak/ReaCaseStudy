using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using static Business.RequestHandlers.Product.AddSupply;

namespace Business.EventHandlers.Kafka;

public class AddSupplyConsumer : BackgroundService
{
    private readonly ILogger<AddSupplyConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IDistributedLockFactory _lockFactory;


    public AddSupplyConsumer(
        ILogger<AddSupplyConsumer> logger,
        IUnitOfWork unitOfWork,
        IKafkaConsumerService kafkaConsumer,
        IDistributedLockFactory lockFactory)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _kafkaConsumer = kafkaConsumer;
        _lockFactory = lockFactory;
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

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date cannot be empty.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //await Task.Run(async () =>
        //{
            await _kafkaConsumer.ConsumeAsync<AddSupplyMessage>(
                "product-add-supply",
                 async (message) => await ProcessProductAddSupply(message, stoppingToken),
                 stoppingToken);
        //});
    }
    private async Task ProcessProductAddSupply(AddSupplyMessage message, CancellationToken cancellationToken)
    {
        var resource = $"product-supply-lock:{message.ProductId}";
        await using var redLock = await _lockFactory.CreateLockAsync(
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


        try
        {
            var validator = new AddSupplyRequestValidator();
            var validationResult = validator.Validate(message);

            if (!validationResult.IsValid)
            {
                // MAIL SECTION
                _logger.LogWarning(validationResult.Errors.First().ErrorMessage);
                return;
            }

            if (await _unitOfWork.Products.CountAsync(msg => msg.Id == message.ProductId) == 0)
            {
                // MAIL SECTION
                _logger.LogWarning("Specified product is not found");
                return;
            }

            var productSupply = new ProductSupply
            {
                ProductId = message.ProductId,
                Quantity = message.Quantity,
                Date = message.Date,
                RemainingQuantity = message.Quantity
            };
            await _unitOfWork.ProductSupplies.AddAsync(productSupply);
            await _unitOfWork.CommitAsync();

            // MAIL SECTION
            _logger.LogInformation("Product suplly addition is successfull");
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
            _logger.LogDebug($"Releasing lock for {resource}");
        }
        
    }
}
