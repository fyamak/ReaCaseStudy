using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Business.RequestHandlers.Product.CreateProduct;

namespace Business.EventHandlers.Kafka;

public class CreateProductConsumer : BackgroundService
{
    private readonly ILogger<CreateProductConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaConsumerService _kafkaConsumer;

    public CreateProductConsumer(
        ILogger<CreateProductConsumer> logger,
        IUnitOfWork unitOfWork,
        IKafkaConsumerService kafkaConsumer)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _kafkaConsumer = kafkaConsumer;
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
        //await Task.Run(async () =>
        //{
            await _kafkaConsumer.ConsumeAsync<CreateProductMessage>(
                "product-create",
                 async (message) => await ProcessProductCreation(message, stoppingToken),
                 stoppingToken);
        //});
    }

    private async Task ProcessProductCreation(CreateProductMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var validator = new CreateProductRequestValidator();
            var validationResult = validator.Validate(message);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(validationResult.Errors.First().ErrorMessage);
                return;
            }

            if (await _unitOfWork.Products.CountAsync(msg => msg.Name == message.Name) > 0)
            {
                // MAIL SECTION
                _logger.LogWarning("Product with same name already exists");
                return;
            }
            var product = new Infrastructure.Data.Postgres.Entities.Product { Name = message.Name };
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CommitAsync();

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
    }
}
