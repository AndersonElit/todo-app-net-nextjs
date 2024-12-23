using TodoApi.Domain.Models;
using TodoApi.Domain.Ports.Out;
using TodoApi.Domain.Ports.In;
using TodoApi.Infrastructure.DrivenAdapters.RabbitMQProducer;
using TodoApi.Infrastructure.MysqlDb.DrivenAdapters;

namespace TodoApi.Application.UseCases
{
    public class TodoUseCase : TodoPort, IDisposable
    {
        private readonly TodoRepository _todoRepository;
        private readonly RabbitMQProducer _rabbitMQProducer;

        public TodoUseCase(TodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
            _rabbitMQProducer = new RabbitMQProducer();
            Console.WriteLine("TodoUseCase initialized with RabbitMQProducer");
        }

        public async Task<IEnumerable<Todo>> GetAllTodos()
        {
            return await _todoRepository.GetAll();
        }

        public async Task<Todo?> GetTodoById(int id)
        {
            return await _todoRepository.GetById(id);
        }

        public async Task<IEnumerable<Todo>> GetTodosByStatus(TodoStatus status)
        {
            return await _todoRepository.GetByStatus(status);
        }

        public async Task<Todo> CreateTodo(Todo todo)
        {
            todo.CreatedAt = DateTime.UtcNow;
            return await _todoRepository.Create(todo);
        }

        public async Task UpdateTodo(int id, Todo todo)
        {
            if (id != todo.Id)
                throw new ArgumentException("Id mismatch");

            await _todoRepository.Update(todo);
        }

        public async Task UpdateTodoStatus(int id, TodoStatus status)
        {
            try
            {
                Console.WriteLine($"Updating todo {id} status to {status}");
                var todo = await _todoRepository.GetById(id);
                if (todo == null)
                    throw new KeyNotFoundException($"Todo with id {id} not found");

                var oldStatus = todo.Status;
                todo.Status = status;
                await _todoRepository.Update(todo);

                Console.WriteLine($"Publishing status update for todo {id}");
                _rabbitMQProducer.PublishStatusUpdate(todo, oldStatus);
                Console.WriteLine("Status update published successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateTodoStatus: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteTodo(int id)
        {
            if (!await _todoRepository.Exists(id))
                throw new KeyNotFoundException($"Todo with id {id} not found");

            await _todoRepository.Delete(id);
        }

        public void Dispose()
        {
            _rabbitMQProducer?.Dispose();
        }
    }
}
