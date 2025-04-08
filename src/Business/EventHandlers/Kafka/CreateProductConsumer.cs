using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Business.RequestHandlers.Product.CreateProduct;

namespace Business.EventHandlers.Kafka;

public class CreateProductConsumer : BackgroundService
{
    private readonly ILogger<CreateProductConsumer> _logger;
    private readonly IKafkaConsumerService _kafkaConsumer;
    private readonly IServiceProvider _serviceProvider;


    public CreateProductConsumer(
        ILogger<CreateProductConsumer> logger,
        IKafkaConsumerService kafkaConsumer,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _kafkaConsumer = kafkaConsumer;
        _serviceProvider = serviceProvider;
    }
    public class CreateProductRequestValidator : AbstractValidator<CreateProductMessage>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2)
                .WithMessage("Name must be at least two length.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _kafkaConsumer.ConsumeAsync<CreateProductMessage>(
            "product-create",
            async (message) => await ProcessProductCreation(message, stoppingToken),
            stoppingToken);
    }

    private async Task ProcessProductCreation(CreateProductMessage message, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        try
        {
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();

            var validator = new CreateProductRequestValidator();
            var validationResult = validator.Validate(message);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(validationResult.Errors.First().ErrorMessage);
                return;
            }

            if (await unitOfWork.Products.CountAsync(msg => msg.Name == message.Name) > 0)
            {
                // MAIL SECTION
                _logger.LogWarning("Product with same name already exists");
                return;
            }
            var product = new Infrastructure.Data.Postgres.Entities.Product { Name = message.Name };
            await unitOfWork.Products.AddAsync(product);
            await unitOfWork.CommitAsync();

            // MAIL SECTION
            _logger.LogInformation("Product successfully created", product);
            return;
        }
        catch (Exception ex)
        {
            // MAIL SECTION
            _logger.LogError(ex, "Error processing product creation", message.Name);
            return;
        }
        finally
        {
            scope.Dispose();
        }
    }
}
