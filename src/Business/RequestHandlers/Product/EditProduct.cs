using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Kafka;
using Business.Services.Kafka.Interface;


namespace Business.RequestHandlers.Product;

public class EditProduct
{
    public class EditProductRequest : IRequest<DataResult<string>>
    {
        public int Id;
        public string Name { get; set; }
    }

    public class EditProductMessage : KafkaMessage
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    

    public class EditProductRequestHandler : IRequestHandler<EditProductRequest, DataResult<string>>
    {
        private readonly ILogger _logger;
        private readonly IKafkaProducerService _kafkaProducer;

        public EditProductRequestHandler(ILogger logger, IKafkaProducerService kafkaProducer)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<DataResult<string>> Handle(EditProductRequest request, CancellationToken cancellationToken)
        {
            
            try
            {
                var message = new EditProductMessage
                {
                    Topic = "product-edit",
                    Id = request.Id,
                    Name = request.Name
                };

                await _kafkaProducer.ProduceAsync(message.Topic, message);
                return DataResult<string>.Success("Product edit request accepted");

            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<string>.Error(ex.Message);
            }
        }
    }
}
