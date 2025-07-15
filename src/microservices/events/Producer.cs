using System.Text;
using Confluent.Kafka;

namespace Events;

public class Producer
{
    private readonly ILogger<Producer> _logger;
    private readonly ProducerConfig _producerConfig;

    public Producer(ILogger<Producer> logger)
    {
        _logger = logger;
        _producerConfig = new ProducerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BROKERS")
        };
    }

    public async Task SendAsync(string message, string topic)
    {
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
        await producer.ProduceAsync(topic, new Message<Null, string>() { Value = message });
        _logger.LogInformation($"Message sent to topic {topic}.");
    }
}