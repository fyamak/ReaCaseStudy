namespace Business.Services.Kafka.Interface;
public interface IKafkaProducerService
{
    Task ProduceAsync<T>(string topic, T message) where T : class;
}
