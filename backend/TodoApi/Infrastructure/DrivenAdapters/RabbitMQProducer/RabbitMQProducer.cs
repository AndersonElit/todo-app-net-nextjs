using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TodoApi.Domain.Models;

namespace TodoApi.Infrastructure.DrivenAdapters.RabbitMQProducer
{
    public class RabbitMQProducer : IDisposable
    {
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQProducer(string hostname = "192.168.1.15", string queueName = "todo_status_updates")
        {
            _hostname = hostname;
            _queueName = queueName;

            var factory = new ConnectionFactory { HostName = _hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
        }

        public void PublishStatusUpdate(Todo todo, TodoStatus oldStatus)
        {
            var message = new
            {
                TodoId = todo.Id,
                Title = todo.Title,
                OldStatus = oldStatus,
                NewStatus = todo.Status,
                UpdatedAt = DateTime.UtcNow
            };

            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublish(exchange: "",
                                routingKey: _queueName,
                                basicProperties: null,
                                body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
