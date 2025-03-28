using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using static Business.RequestHandlers.Product.AddSales;

namespace Business.EventHandlers.Kafka;

public class AddSaleConsumer : BackgroundService
{
    private readonly ILogger<AddSaleConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IDistributedLockFactory _lockFactory;

    public AddSaleConsumer(
        ILogger<AddSaleConsumer> logger,
        IUnitOfWork unitOfWork,
        IKafkaConsumerService kafkaConsumer,
        IDistributedLockFactory lockFactory)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _kafkaConsumer = kafkaConsumer;
        _lockFactory = lockFactory;
    }
    public class AddSalesRequestValidator : AbstractValidator<AddSaleMessage>
    {
        public AddSalesRequestValidator()
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
            await _kafkaConsumer.ConsumeAsync<AddSaleMessage>(
                "product-add-sale",
                async (message) => await ProcessProductAddSale(message, stoppingToken),
                stoppingToken);
        //});
    }

    private async Task ProcessProductAddSale(AddSaleMessage message, CancellationToken cancellationToken)
    {
        var resource = $"product-sale-lock:{message.ProductId}";
        await using var redLock = await _lockFactory.CreateLockAsync(
            resource,
            TimeSpan.FromSeconds(30), // lock expiration time
            TimeSpan.FromSeconds(5), // wait time
            TimeSpan.FromMilliseconds(500), // retry interval
            cancellationToken);

        if (!redLock.IsAcquired)
        {
            _logger.LogWarning($"Could not acquire lock for product sale: {message.ProductId}");
            return;
        }


        try
        {
            var validator = new AddSalesRequestValidator();
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                // MAIL SECTION
                _logger.LogWarning(validationResult.Errors.First().ErrorMessage);
                return;
            }

            if (await _unitOfWork.Products.CountAsync(msg => msg.Id == message.ProductId) == 0)
            {
                //MAIL SECTION
                _logger.LogWarning("Specified product is not found");
                return;
            }

            //// .ContinueWith() can be used
            //var productSupplies = await _unitOfWork.ProductSupplies
            //    .FindAsync(ps => ps.ProductId == message.ProductId && ps.RemainingQuantity > 0 && ps.Date < message.Date)
            //    .ContinueWith(ps => ps.Result.OrderBy(ps => ps.Date).ToList());

            var productSupplies = await _unitOfWork.ProductSupplies
                .FindAsync(ps => ps.ProductId == message.ProductId && ps.RemainingQuantity > 0 && ps.Date < message.Date);
            var orderedProductSupplies = productSupplies.OrderBy(ps => ps.Date).ToList();


            var totalAvailableStock = orderedProductSupplies.Sum(ps => ps.RemainingQuantity);
            if (totalAvailableStock < message.Quantity)
            {
                _logger.LogWarning("Insufficient stock to complete the sale");
                return;
            }


            var saleQuantity = message.Quantity;
            foreach (var orderedProductSupply in orderedProductSupplies)
            {
                if (saleQuantity == 0)
                    break;

                if (orderedProductSupply.RemainingQuantity >= saleQuantity)
                {
                    orderedProductSupply.RemainingQuantity -= saleQuantity;
                    saleQuantity = 0;
                }
                else
                {
                    saleQuantity -= orderedProductSupply.RemainingQuantity;
                    orderedProductSupply.RemainingQuantity = 0;
                }
                
                await _unitOfWork.ProductSupplies.Update(orderedProductSupply);
            }

            var productSale = new ProductSale
            {
                ProductId = message.ProductId,
                Quantity = message.Quantity,
                Date = message.Date
            };

            await _unitOfWork.ProductSales.AddAsync(productSale);
            await _unitOfWork.CommitAsync();

            //MAIL SECTION
            _logger.LogInformation("Product sale addition is successfull");
            return;
        }
        catch(Exception ex)
        {
            // MAIL SECTION
            _logger.LogError(ex, "Error processing product sale addition", message.ProductId);
            return;
        }
        finally
        {
            _logger.LogDebug($"Releasing lock for {resource}");
        }
    }
}
