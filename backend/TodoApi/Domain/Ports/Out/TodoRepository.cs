using TodoApi.Domain.Models;

namespace TodoApi.Domain.Ports.Out
{
    public interface TodoRepository
    {
        Task<IEnumerable<Todo>> GetAll();
        Task<Todo?> GetById(int id);
        Task<IEnumerable<Todo>> GetByStatus(TodoStatus status);
        Task<Todo> Create(Todo todo);
        Task Update(Todo todo);
        Task Delete(int id);
        Task<bool> Exists(int id);
    }
}
