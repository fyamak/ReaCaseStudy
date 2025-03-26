//using Business.Services.Kafka.Interface;
//using FluentValidation;
//using Infrastructure.Data.Postgres;
//using Infrastructure.Data.Postgres.Entities;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Shared.Models.Kafka;
//using static Business.RequestHandlers.Product.AddSupply;
//using static Business.RequestHandlers.Product.CreateProduct;

//namespace Business.Services.Background.Kafka;

//public class Consumer : BackgroundService
//{

//    private readonly ILogger<Consumer> _logger;
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly IKafkaConsumer _kafkaConsumer;

//    public Consumer(
//        ILogger<Consumer> logger,
//        IUnitOfWork unitOfWork,
//        IKafkaConsumer kafkaConsumer)
//    {
//        _logger = logger;
//        _unitOfWork = unitOfWork;
//        _kafkaConsumer = kafkaConsumer;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        // Process in parallel
//        var tasks = new[]
//        {
//            ProcessTopic<CreateProductMessage>("product-create", ProcessProductCreation, stoppingToken),
//            ProcessTopic<AddSupplyMessage>("product-add-supply", ProcessProductAddSupply, stoppingToken),
//        };

//        await Task.WhenAll(tasks);
//    }

//    private async Task ProcessTopic<T>(string topic, Func<T, CancellationToken, Task> processor, CancellationToken cancellationToken) where T : KafkaMessage
//    {
//        await Task.Run(async () =>
//        {
//            await _kafkaConsumer.ConsumeAsync<T>(
//                topic,
//                async (message) => await processor(message, cancellationToken),
//                cancellationToken);
//        });
//    }

//    private async Task ProcessProductCreation(CreateProductMessage message, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (await _unitOfWork.Products.CountAsync(p => p.Name == message.Name) > 0)
//            {
//                // MAIL SECTION
//                _logger.LogWarning("Product with same name already exists");
//                return;
//            }
//            var product = new Product { Name = message.Name };
//            await _unitOfWork.Products.AddAsync(product);
//            await _unitOfWork.CommitAsync();

//            // MAIL SECTION
//            _logger.LogInformation("Product successfully created", product);
//            return;
//        }
//        catch (Exception ex)
//        {
//            // MAIL SECTION
//            _logger.LogError(ex, "Error processing product creation", message.Name);
//            return;
//        }
//    }

//    private async Task ProcessProductAddSupply(AddSupplyMessage message, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (await _unitOfWork.Products.CountAsync(p => p.Id == message.ProductId) == 0)
//            {
//                // MAIL SECTION
//                _logger.LogWarning("Specified product is not found");
//                return;
//            }

//            var productSupply = new ProductSupply
//            {
//                ProductId = message.ProductId,
//                Quantity = message.Quantity,
//                Date = message.Date,
//                RemainingQuantity = message.Quantity
//            };
//            await _unitOfWork.ProductSupplies.AddAsync(productSupply);
//            await _unitOfWork.CommitAsync();

//            // MAIL SECTION
//            _logger.LogInformation("Product suplly addition is successfull");
//            return;
//        }
//        catch (Exception ex)
//        {
//            // MAIL SECTION
//            _logger.LogError(ex, "Error processing product suplly addition", message.ProductId);
//        }
//    }

//}
