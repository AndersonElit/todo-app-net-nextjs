using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TodoApi.Infrastructure.EntryPoints.RabbitMQConsumer
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _hostname;

        public RabbitMQConsumer(string queueName = "todo_status_updates")
        {
            _hostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            _queueName = queueName;

            Console.WriteLine($"Initializing RabbitMQ Consumer with host: {_hostname}");
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
                
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                Console.WriteLine("RabbitMQ Consumer initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing RabbitMQ Consumer: {ex.Message}");
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Received message: {message}");
                        
                        var update = JsonSerializer.Deserialize<TodoStatusUpdate>(message);

                        if (update != null)
                        {
                            Console.WriteLine($"\nTodo Status Update Received:");
                            Console.WriteLine($"Todo ID: {update.TodoId}");
                            Console.WriteLine($"Title: {update.Title}");
                            Console.WriteLine($"Status changed from {update.OldStatus} to {update.NewStatus}");
                            Console.WriteLine($"Updated at: {update.UpdatedAt}\n");
                        }

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                _channel.BasicConsume(queue: _queueName,
                                    autoAck: false,
                                    consumer: consumer);

                Console.WriteLine("Started consuming messages");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExecuteAsync: {ex.Message}");
                throw;
            }
        }

        public override void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                base.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing RabbitMQ connections: {ex.Message}");
            }
        }
    }

    public class TodoStatusUpdate
    {
        public int TodoId { get; set; }
        public required string Title { get; set; }
        public required string OldStatus { get; set; }
        public required string NewStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
