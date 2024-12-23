using TodoApi.Domain.Models;
using TodoApi.Domain.Ports.Out;
using TodoApi.Domain.Ports.In;
using TodoApi.Infrastructure.DrivenAdapters.RabbitMQProducer;

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

            // Publish status update to RabbitMQ
            _rabbitMQProducer.PublishStatusUpdate(todo, todo.Status);
        }

        public async Task UpdateTodoStatus(int id, TodoStatus status)
        {
            var todo = await _todoRepository.GetById(id);
            if (todo == null)
                throw new KeyNotFoundException($"Todo with id {id} not found");

            var oldStatus = todo.Status;
            todo.Status = status;
            await _todoRepository.Update(todo);

            // Publish status update to RabbitMQ
            _rabbitMQProducer.PublishStatusUpdate(todo, oldStatus);
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
