using Business.Services.Kafka.Interface;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Kafka;
using Shared.Models.Results;

namespace Business.RequestHandlers.Organization;

public abstract class CreateOrganization
{
    public class CreateOrganizationRequest : IRequest<DataResult<string>>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class CreateOrganizationMessage : KafkaMessage
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class CreateOrganizationRequestHandler : IRequestHandler<CreateOrganizationRequest, DataResult<string>>
    {
        private readonly ILogger _logger;
        private readonly IKafkaProducerService _kafkaProducer;

        public CreateOrganizationRequestHandler(ILogger logger, IKafkaProducerService kafkaProducer)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<DataResult<string>> Handle(CreateOrganizationRequest request, CancellationToken cancellationToken)
        {

            try
            {
                var message = new CreateOrganizationMessage
                {
                    Topic = "organization-create",
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address
                };

                await _kafkaProducer.ProduceAsync(message.Topic, message);
                return DataResult<string>.Success("Organization creation request accepted");
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<string>.Error(ex.Message);
            }
        }
    }
}
