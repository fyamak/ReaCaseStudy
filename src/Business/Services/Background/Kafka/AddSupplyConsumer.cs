using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Business.RequestHandlers.Product.AddSupply;

namespace Business.Services.Background.Kafka;

public class AddSupplyConsumer : BackgroundService
{
    private readonly ILogger<AddSupplyConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaConsumer _kafkaConsumer;

    public AddSupplyConsumer(
        ILogger<AddSupplyConsumer> logger,
        IUnitOfWork unitOfWork, 
        IKafkaConsumer kafkaConsumer)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _kafkaConsumer = kafkaConsumer;
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

            if (await _unitOfWork.Products.CountAsync(p => p.Id == message.ProductId) == 0)
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
        }
    }
}
