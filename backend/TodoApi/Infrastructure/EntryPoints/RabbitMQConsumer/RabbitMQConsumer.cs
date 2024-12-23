using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TodoApi.Infrastructure.EntryPoints.RabbitMQConsumer
{
    public class RabbitMQConsumer : BackgroundService
     {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        
        public RabbitMQConsumer(string hostname = "192.168.1.15", string queueName = "todo_status_updates")
        {
            _queueName = queueName;
            
            var factory = new ConnectionFactory { HostName = hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var update = JsonSerializer.Deserialize<TodoStatusUpdate>(message);
                
                if (update != null)  // Null check added here
                {
                    Console.WriteLine($"\nTodo Status Update Received:");
                    Console.WriteLine($"Todo ID: {update.TodoId}");
                    Console.WriteLine($"Title: {update.Title}");
                    Console.WriteLine($"Status changed from {update.OldStatus} to {update.NewStatus}");
                    Console.WriteLine($"Updated at: {update.UpdatedAt}\n");
                }
            };
            
            _channel.BasicConsume(queue: _queueName,
                                autoAck: true,
                                consumer: consumer);
            
            return Task.CompletedTask;
        }
        
        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
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
