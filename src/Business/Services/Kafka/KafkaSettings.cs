using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Business.Services.Kafka;
public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "kafka:9092";
    //public string GroupId { get; set; }
    //public bool EnableAutoCommit { get; set; }
    //public int SessionTimeoutMs { get; set; }
    //public string ClientId { get; set; }
    //public bool AllowAutoCreateTopics { get; set; }
}
