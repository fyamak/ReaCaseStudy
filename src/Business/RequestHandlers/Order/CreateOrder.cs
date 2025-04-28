using Business.Services.Kafka.Interface;
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
    public class CreateOrderRequest : IRequest<DataResult<string>>
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
