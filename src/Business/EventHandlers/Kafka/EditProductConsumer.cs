using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using static Business.RequestHandlers.Product.EditProduct;

namespace Business.EventHandlers.Kafka;

public class EditProductConsumer : BackgroundService
{
    private readonly ILogger<EditProductConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IDistributedLockFactory _lockFactory;

    public EditProductConsumer(
        ILogger<EditProductConsumer> logger,
        IUnitOfWork unitOfWork,
        IKafkaConsumerService kafkaConsumer,
        IDistributedLockFactory lockFactory)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _kafkaConsumer = kafkaConsumer;
        _lockFactory = lockFactory;
    }

    public class EditProductRequestValidator : AbstractValidator<EditProductMessage>
    {
        public EditProductRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be empty.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //await Task.Run(async () =>
        //{
            await _kafkaConsumer.ConsumeAsync<EditProductMessage>(
                "product-edit",
                 async (message) => await ProcessProductEdit(message, stoppingToken),
                 stoppingToken);
        //});
    }

    private async Task ProcessProductEdit(EditProductMessage message, CancellationToken cancellationToken)
    {
        var resource = $"product-edit-lock:{message.Id}";
        await using var redLock = await _lockFactory.CreateLockAsync(
            resource,
            TimeSpan.FromSeconds(10), // lock expiration time
            TimeSpan.FromSeconds(3), // wait time
            TimeSpan.FromMilliseconds(500), // retry interval
            cancellationToken);

        if (!redLock.IsAcquired)
        {
            _logger.LogWarning($"Could not acquire lock for product edit: {message.Id}");
            return;
        }

        try
        {
            var validator = new EditProductRequestValidator();
            var validationResult = validator.Validate(message);

            if (!validationResult.IsValid)
            {
                // MAIL SECTION
                _logger.LogWarning(validationResult.Errors.First().ErrorMessage);
                return;
            }

            var product = await _unitOfWork.Products.FirstOrDefaultAsync(msg => msg.Id == message.Id && !msg.IsDeleted);

            if (product == null)
            {
                // MAIL SECTION
                _logger.LogWarning("Invalid product id");
                return;
            }

            if (await _unitOfWork.Products.CountAsync(msg => msg.Name == message.Name) > 0)
            {
                // MAIL SECTION
                _logger.LogWarning("Product with same name already exists");
                return;
            }

            product.Name = message.Name;
            product.UpdatedAt = DateTime.UtcNow;

            var result = await _unitOfWork.Products.Update(product);

            // MAIL SECTION
            _logger.LogInformation("Product is updated successfully");
            return;
        }
        catch(Exception ex)
        {
            // MAIL SECTION
            _logger.LogError(ex, "Error processing product creation", message.Id);
            return;
        }

    }
}
