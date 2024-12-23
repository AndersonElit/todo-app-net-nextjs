using TodoApi.Domain.Models;

namespace TodoApi.Domain.Ports.In
{
    public interface TodoPort
    {
        Task<IEnumerable<Todo>> GetAllTodos();
        Task<Todo?> GetTodoById(int id);
        Task<IEnumerable<Todo>> GetTodosByStatus(TodoStatus status);
        Task<Todo> CreateTodo(Todo todo);
        Task UpdateTodo(int id, Todo todo);
        Task UpdateTodoStatus(int id, TodoStatus status);
        Task DeleteTodo(int id);
    }
}
