using Business.Mediator.Behaviours.Requests;
using Business.Services.Kafka.Interface;
using FluentValidation;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Kafka;
using Shared.Models.Results;

namespace Business.RequestHandlers.Product
{
    public class AddSales
    {
        public class AddSalesRequest : IRequest<DataResult<string>>, IRequestToValidate
        {
            public int ProductId;
            public int OrganizationId { get; set; }
            public int Quantity { get; set; }
            public double Price { get; set; }
            public DateTime Date { get; set; }
            public int OrderId { get; set; }
        }
        public class AddSaleMessage : KafkaMessage
        {
            public int ProductId { get; set; }
            public int OrganizationId { get; set; }
            public int Quantity { get; set; }
            public double Price { get; set; }
            public DateTime Date { get; set; }
            public int OrderId { get; set; }
        }

        public class AddSalesRequestValidator : AbstractValidator<AddSalesRequest>
        {
            public AddSalesRequestValidator()
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

                RuleFor(x => x.OrderId)
                    .NotEmpty().WithMessage("Order Id must not be empty.");
            }
        }


        public class AddSalesRequestHandler : IRequestHandler<AddSalesRequest, DataResult<string>>
        {
            private readonly ILogger _logger;
            private readonly IKafkaProducerService _kafkaProducer;
            public AddSalesRequestHandler(ILogger logger, IKafkaProducerService kafkaProducer)
            {
                _logger = logger;
                _kafkaProducer = kafkaProducer;
            }

            public async Task<DataResult<string>> Handle(AddSalesRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    var message = new AddSaleMessage
                    {
                        Topic = "product-add-sale",
                        ProductId = request.ProductId,
                        OrganizationId = request.OrganizationId,
                        Quantity = request.Quantity,
                        Price = request.Price,
                        Date = request.Date,
                        OrderId = request.OrderId
                    };

                    await _kafkaProducer.ProduceAsync(message.Topic, message);
                    return DataResult<string>.Success("Adding sale to product request accepted");
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                    return DataResult<string>.Error(ex.Message);
                }
            }
        }
    }
}
