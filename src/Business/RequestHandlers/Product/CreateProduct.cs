using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Infrastructure.Data.Postgres;
using Shared.Models.Kafka;
using Business.Services.Kafka.Interface;
using FluentValidation;
using Business.Mediator.Behaviours.Requests;

namespace Business.RequestHandlers.Product
{
    public abstract class CreateProduct
    {
        public class CreateProductRequest : IRequest<DataResult<string>>, IRequestToValidate
        {
            public string SKU { get; set; }
            public string Name { get; set; }
            public int CategoryId { get; set; }
        }

        public class CreateProductMessage : KafkaMessage
        {
            public string SKU { get; set; }
            public string Name { get; set; }
            public int CategoryId { get; set; }
        }

        public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
        {
            public CreateProductRequestValidator()
            {
                RuleFor(x => x.SKU).NotEmpty().WithMessage("Please enter a SKU.");
                RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter a SKU.");
                RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("CategoryId must be greater than zero.");
            }
        }


        public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, DataResult<string>>
        {
            private readonly ILogger _logger;
            private readonly IKafkaProducerService _kafkaProducer;

            public CreateProductRequestHandler(ILogger logger, IUnitOfWork unitOfWork, IKafkaProducerService kafkaProducer)
            {
                _logger = logger;
                _kafkaProducer = kafkaProducer;
            }

            public async Task<DataResult<string>> Handle(CreateProductRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    var message = new CreateProductMessage
                    {
                        Topic = "product-create",
                        Name = request.Name,
                        SKU = request.SKU,
                        CategoryId = request.CategoryId,
                    };

                    await _kafkaProducer.ProduceAsync(message.Topic, message);
                    return DataResult<string>.Success("Product creation request accepted");
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                    return  DataResult<string>.Error(ex.Message);
                }
            }
        }
    }
}
