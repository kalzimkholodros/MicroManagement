namespace ClassService.Services;

public interface IRabbitMQService
{
    void PublishMessage(string message, string routingKey);
    void Subscribe(string queueName, string routingKey, Action<string> messageHandler);
} 