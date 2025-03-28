using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Business.Services.Kafka.Interface;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Business.Services.Kafka;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly ProducerConfig _config;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IOptions<KafkaSettings> settings, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _config = new ProducerConfig
        {   
            BootstrapServers = settings.Value.BootstrapServers,
            ClientId = $"producer-{Environment.MachineName}",
            Acks = Acks.All,
            AllowAutoCreateTopics = true
        };
    }

    public async Task ProduceAsync<T>(string topic, T message) where T : class
    {
        try
        {
            using var producer = new ProducerBuilder<Null, string>(_config).Build();
            var serializedMessage = JsonSerializer.Serialize(message);

            var result = await producer.ProduceAsync(
                topic, 
                new Message<Null, string> { Value = serializedMessage }
                );

            _logger.LogInformation($"Message delivered to '{result.Topic}' at partition {result.Partition} with offset {result.Offset}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error producing message to topic {topic}: {ex.Message}");
            throw;
        }
    }
}
