using Confluent.Kafka;

namespace Events;

public class Consumer : BackgroundService
{
    private readonly ILogger<Consumer> _logger;
    private readonly ConsumerConfig _consumerConfig;

    public Consumer(ILogger<Consumer> logger)
    {
        _logger = logger;
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BROKERS"),
            GroupId = "1",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consumer running at: {time}", DateTimeOffset.Now);
        await Task.Run(() => Consume(stoppingToken), stoppingToken);
        _logger.LogInformation("debug {time}", DateTimeOffset.Now);
    }

    public async Task Consume(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
                consumer.Subscribe(["user-events", "payment-events", "movie-events"]);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);

                    _logger.LogInformation("Received message from topic '{Topic}':{Message}",
                        consumeResult.Topic,
                        consumeResult.Message.Value);
                }

                consumer.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "consume error");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }
}