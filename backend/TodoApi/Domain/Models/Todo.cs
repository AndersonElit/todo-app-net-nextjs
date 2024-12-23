namespace TodoApi.Domain.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TodoStatus Status { get; set; } = TodoStatus.Todo;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
