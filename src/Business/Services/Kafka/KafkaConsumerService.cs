using System.Text.Json;
using Business.Services.Kafka.Interface;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Business.Services.Kafka;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly ConsumerConfig _config;
    private readonly ILogger<KafkaConsumerService> _logger;
    public KafkaConsumerService(IOptions<KafkaSettings> settings, ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;
        _config = new ConsumerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            GroupId = $"consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            SessionTimeoutMs = 20000
        };
    }

    public async Task ConsumeAsync<T>(string topic, Action<T> messageHandler, CancellationToken cancellationToken) where T : class
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
        consumer.Subscribe(topic);

        try
        {
            await Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        if (consumeResult != null)
                        {
                            _logger.LogInformation($"Consumed message '{consumeResult.Message.Value}' from topic {consumeResult.Topic}");

                            var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                            messageHandler(message);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError($"Consume error: {ex.Message}");
                    }
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
        finally
        {
            consumer.Close();
        }

        await Task.CompletedTask;
    }
}
