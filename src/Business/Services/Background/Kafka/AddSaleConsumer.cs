using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Business.RequestHandlers.Product.AddSales;

namespace Business.Services.Background.Kafka;

public class AddSaleConsumer : BackgroundService
{
    private readonly ILogger<AddSaleConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaConsumer _kafkaConsumer;
    public AddSaleConsumer(
        ILogger<AddSaleConsumer> logger, 
        IUnitOfWork unitOfWork, 
        IKafkaConsumer kafkaConsumer)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _kafkaConsumer = kafkaConsumer;
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

            if (await _unitOfWork.Products.CountAsync(p => p.Id == message.ProductId) == 0)
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
    }
}
