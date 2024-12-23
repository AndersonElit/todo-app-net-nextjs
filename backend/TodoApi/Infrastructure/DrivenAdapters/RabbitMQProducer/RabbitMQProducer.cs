using System;
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

        public RabbitMQProducer(string queueName = "todo_status_updates")
        {
            _hostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            _queueName = queueName;

            Console.WriteLine($"Initializing RabbitMQ Producer with host: {_hostname}");
            var factory = new ConnectionFactory 
            { 
                HostName = _hostname,
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
                Console.WriteLine("RabbitMQ Producer initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing RabbitMQ Producer: {ex.Message}");
                throw;
            }
        }

        public void PublishStatusUpdate(Todo todo, TodoStatus oldStatus)
        {
            try
            {
                var message = new
                {
                    TodoId = todo.Id,
                    Title = todo.Title,
                    OldStatus = oldStatus.ToString(),
                    NewStatus = todo.Status.ToString(),
                    UpdatedAt = DateTime.UtcNow
                };

                var jsonMessage = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                Console.WriteLine($"Publishing message: {jsonMessage}");
                _channel.BasicPublish(exchange: "",
                                    routingKey: _queueName,
                                    basicProperties: properties,
                                    body: body);
                Console.WriteLine("Message published successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing RabbitMQ connections: {ex.Message}");
            }
        }
    }
}
