using Business.Services.Kafka.Interface;
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
        public class AddSalesRequest : IRequest<DataResult<string>>
        {
            public int ProductId;
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
        }
        public class AddSaleMessage : KafkaMessage
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
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
                        Quantity = request.Quantity,
                        Date = request.Date
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
