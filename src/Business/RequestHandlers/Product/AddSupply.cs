using Infrastructure.Data.Postgres;
using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Infrastructure.Data.Postgres.Entities;
using FluentValidation;
using Shared.Models.Kafka;
using Business.Services.Kafka.Interface;
using Business.Services.Redis.Interface;


namespace Business.RequestHandlers.Product
{
    public class AddSupply
    {
        public class AddSupplyRequest : IRequest<DataResult<string>>
        {
            public int ProductId;
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
        }

        public class AddSupplyMessage : KafkaMessage
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
        }

        public class AddSupplyRequestHandler : IRequestHandler<AddSupplyRequest, DataResult<string>>
        {
            private readonly ILogger _logger;
            private readonly IKafkaProducerService _kafkaProducer;
            public AddSupplyRequestHandler(ILogger logger, IKafkaProducerService kafkaProducer)
            {
                _logger = logger;
                _kafkaProducer = kafkaProducer;
            }

            public async Task<DataResult<string>> Handle(AddSupplyRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    var message = new AddSupplyMessage
                    {
                        Topic = "product-add-supply",
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        Date = request.Date
                    };

                    await _kafkaProducer.ProduceAsync(message.Topic, message);
                    return DataResult<string>.Success("Adding supply to product request accepted");

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
