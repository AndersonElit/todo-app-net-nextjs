using Microsoft.EntityFrameworkCore;
using TodoApi.Domain.Models;
using TodoApi.Domain.Ports.Out;

namespace TodoApi.Infrastructure.MysqlDb.DrivenAdapters
{
    public class TodoAdapter : TodoRepository
    {
        private readonly TodoDbContext _context;

        public TodoAdapter(TodoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Todo>> GetAll()
        {
            return await _context.Todos.ToListAsync();
        }

        public async Task<Todo?> GetById(int id)
        {
            return await _context.Todos.FindAsync(id);
        }

        public async Task<IEnumerable<Todo>> GetByStatus(TodoStatus status)
        {
            return await _context.Todos
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        public async Task<Todo> Create(Todo todo)
        {
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            return todo;
        }

        public async Task Update(Todo todo)
        {
            _context.Entry(todo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var todo = await GetById(id);
            if (todo != null)
            {
                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Todos.AnyAsync(t => t.Id == id);
        }
    }
}
