using Business.Mediator.Behaviours.Requests;
using Business.Services.Kafka.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Kafka;
using Shared.Models.Results;

namespace Business.RequestHandlers.Order;

public abstract class CreateOrder
{
    public class CreateOrderRequest : IRequest<DataResult<string>>, IRequestToValidate
    {
        public int ProductId { get; set; }
        public int OrganizationId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }

    public class CreateOrderMessage : KafkaMessage
    {
        public int ProductId { get; set; }
        public int OrganizationId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }

    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.ProductId)
                    .NotEmpty().WithMessage("Product Id must not be empty.");

            RuleFor(x => x.OrganizationId)
                .NotEmpty().WithMessage("Organization Id must not be empty.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date must not be empty.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type must not be empty.")
                .Must(type => type == "supply" || type == "sale")
                .WithMessage("Type must be either 'supply' or 'sale'.");
        }
    }


    public class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, DataResult<string>>
    {
        private readonly ILogger _logger;
        private readonly IKafkaProducerService _kafkaProducer;

        public CreateOrderRequestHandler(ILogger logger, IUnitOfWork unitOfWork, IKafkaProducerService kafkaProducer)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<DataResult<string>> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var message = new CreateOrderMessage
                {
                    Topic = "order-create",
                    ProductId = request.ProductId,
                    OrganizationId = request.OrganizationId,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    Date = request.Date,
                    Type = request.Type
                };

                await _kafkaProducer.ProduceAsync(message.Topic, message);
                return DataResult<string>.Success("Order creation request accepted");
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<string>.Error(ex.Message);
            }
        }
    }
}
