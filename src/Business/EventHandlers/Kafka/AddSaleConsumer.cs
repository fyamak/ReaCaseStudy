using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using static Business.RequestHandlers.Product.AddSales;

namespace Business.EventHandlers.Kafka;

public class AddSaleConsumer : BackgroundService
{
    private readonly ILogger<AddSaleConsumer> _logger;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IServiceProvider _serviceProvider;

    public AddSaleConsumer(
        ILogger<AddSaleConsumer> logger,
        IKafkaConsumerService kafkaConsumer,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _kafkaConsumer = kafkaConsumer;
        _serviceProvider = serviceProvider;
    }
    public class AddSalesRequestValidator : AbstractValidator<AddSaleMessage>
    {
        public AddSalesRequestValidator()
        {
            RuleFor(x => x.Quantity)
             .GreaterThan(0)
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
        await _kafkaConsumer.ConsumeAsync<AddSaleMessage>(
            "product-add-sale",
            async (message) => await ProcessProductAddSale(message, stoppingToken),
            stoppingToken);
    }

    private async Task ProcessProductAddSale(AddSaleMessage message, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();

        try
        {
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
            var lockFactory = scope.ServiceProvider.GetService<IDistributedLockFactory>();

            
            
            var resource = $"product-sale-lock:{message.ProductId}";
            await using var redLock = await lockFactory.CreateLockAsync(
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


            var order = await unitOfWork.Orders.GetByIdAsync(message.OrderId);
            if (order == null)
            {
                // MAIL SECTION
                _logger.LogWarning($"Order {message.OrderId} is already processed");
                return;
            }


            var validator = new AddSalesRequestValidator();
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                // MAIL SECTION
                var validationErrorMessage = validationResult.Errors.First().ErrorMessage;
                _logger.LogWarning(validationErrorMessage);
                await FailOrderAsync(order, $"Credentials are invalid. {validationErrorMessage}", unitOfWork);
                return;
            }


            var product = await unitOfWork.Products.GetByIdAsync(message.ProductId);
            if (product == null)
            {
                //MAIL SECTION
                _logger.LogWarning("Specified product is not found");
                await FailOrderAsync(order, "Selected product is not in stock", unitOfWork);
                return;
            }


            var productSupplies = await unitOfWork.ProductSupplies
                .FindAsync(ps => ps.ProductId == message.ProductId && ps.RemainingQuantity > 0 && ps.Date < message.Date);
            var orderedProductSupplies = productSupplies.OrderBy(ps => ps.Date).ToList();


            var totalAvailableStock = orderedProductSupplies.Sum(ps => ps.RemainingQuantity);
            if (totalAvailableStock < message.Quantity)
            {
                _logger.LogWarning("Insufficient stock to complete the sale");
                await FailOrderAsync(order, "Insufficient stock to complete the sale", unitOfWork);
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

                await unitOfWork.ProductSupplies.Update(orderedProductSupply);
            }

            product.TotalQuantity -= message.Quantity;
            await unitOfWork.Products.Update(product);

            var productSale = new ProductSale
            {
                ProductId = message.ProductId,
                OrganizationId = message.OrganizationId,
                Quantity = message.Quantity,
                Price = message.Price,
                Date = message.Date
            };

            order.IsSuccessfull = true;
            order.Detail = "Product sale is successfull";
            order.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.ProductSales.AddAsync(productSale);
            await unitOfWork.Orders.SoftDelete(order);
            await unitOfWork.CommitAsync();

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
            _logger.LogDebug($"Releasing product-sale-lock:{message.ProductId}");
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
