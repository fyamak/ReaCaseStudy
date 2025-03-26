using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using Shared.Models.Results;
using static Business.RequestHandlers.Product.CreateProduct;
using static Business.RequestHandlers.Product.EditProduct;

namespace Business.Services.Background.Kafka;

public class EditProductConsumer : BackgroundService
{
    private readonly ILogger<EditProductConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaConsumer _kafkaConsumer;
    public EditProductConsumer(
        ILogger<EditProductConsumer> logger,
        IUnitOfWork unitOfWork,
        IKafkaConsumer kafkaConsumer)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _kafkaConsumer = kafkaConsumer;
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

            int result = await _unitOfWork.Products.Update(product);

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
