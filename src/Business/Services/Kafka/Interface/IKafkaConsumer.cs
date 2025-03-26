using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.Kafka.Interface;

public interface IKafkaConsumer
{
    Task ConsumeAsync<T>(string topic, Action<T> messageHandler, CancellationToken cancellationToken) where T : class;
}
