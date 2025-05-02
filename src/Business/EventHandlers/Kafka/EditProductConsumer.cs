using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using static Business.RequestHandlers.Product.EditProduct;

namespace Business.EventHandlers.Kafka;

public class EditProductConsumer : BackgroundService
{
    private readonly ILogger<EditProductConsumer> _logger;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IServiceProvider _serviceProvider;


    public EditProductConsumer(
        ILogger<EditProductConsumer> logger,
        IKafkaConsumerService kafkaConsumer,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _kafkaConsumer = kafkaConsumer;
        _serviceProvider = serviceProvider;
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
        await _kafkaConsumer.ConsumeAsync<EditProductMessage>(
            "product-edit",
            async (message) => await ProcessProductEdit(message, stoppingToken),
            stoppingToken);
    }

    private async Task ProcessProductEdit(EditProductMessage message, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();

        try
        {
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
            var lockFactory = scope.ServiceProvider.GetService<IDistributedLockFactory>();

            var resource = $"product-edit-lock:{message.Id}";
            await using var redLock = await lockFactory.CreateLockAsync(
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
            var validator = new EditProductRequestValidator();
            var validationResult = validator.Validate(message);

            if (!validationResult.IsValid)
            {
                // MAIL SECTION
                _logger.LogWarning(validationResult.Errors.First().ErrorMessage);
                return;
            }

            var product = await unitOfWork.Products.FirstOrDefaultAsync(msg => msg.Id == message.Id && !msg.IsDeleted);

            if (product == null)
            {
                // MAIL SECTION
                _logger.LogWarning("Invalid product id");
                return;
            }

            if (await unitOfWork.Products.CountAsync(msg => msg.Name == message.Name) > 0)
            {
                // MAIL SECTION
                _logger.LogWarning("Product with same name already exists");
                return;
            }

            product.Name = message.Name;
            await unitOfWork.Products.Update(product);
            await unitOfWork.CommitAsync();

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
        finally
        {
            _logger.LogDebug($"Releasing product-edit-lock:{message.Id}");
            scope.Dispose();
        }

    }
}
