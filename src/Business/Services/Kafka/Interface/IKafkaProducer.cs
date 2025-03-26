namespace Business.Services.Kafka.Interface;
public interface IKafkaProducer
{
    Task ProduceAsync<T>(string topic, T message) where T : class;
}
