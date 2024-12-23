using Microsoft.AspNetCore.Mvc;
using TodoApi.Domain.Ports.In;
using TodoApi.Domain.Models;

namespace TodoApi.Infrastructure.EntryPoints.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoPort _todoPort;

        public TodoController(TodoPort todoPort)
        {
            _todoPort = todoPort;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
        {
            var todos = await _todoPort.GetAllTodos();
            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var todo = await _todoPort.GetTodoById(id);
            if (todo == null)
                return NotFound();

            return Ok(todo);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodosByStatus(TodoStatus status)
        {
            var todos = await _todoPort.GetTodosByStatus(status);
            return Ok(todos);
        }

        [HttpPost]
        public async Task<ActionResult<Todo>> CreateTodo(Todo todo)
        {
            var createdTodo = await _todoPort.CreateTodo(todo);
            return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, Todo todo)
        {
            try
            {
                await _todoPort.UpdateTodo(id, todo);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTodoStatus(int id, [FromBody] UpdateTodoStatusRequest request)
        {
            try
            {
                await _todoPort.UpdateTodoStatus(id, request.Status);
                var updatedTodo = await _todoPort.GetTodoById(id);
                return Ok(updatedTodo);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            try
            {
                await _todoPort.DeleteTodo(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
