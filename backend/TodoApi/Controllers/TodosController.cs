using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TodosController : ControllerBase
{
    private readonly ITodoRepository _repository;
    private readonly ILogger<TodosController> _logger;

    public TodosController(ITodoRepository repository, ILogger<TodosController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Returns all todo items ordered by creation date.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var todos = _repository.GetAll()
            .Select(MapToResponse);

        return Ok(todos);
    }

    /// <summary>Returns a single todo by its ID.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var item = _repository.GetById(id);
        return item is null ? NotFound() : Ok(MapToResponse(item));
    }

    /// <summary>Creates a new todo item.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateTodoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title cannot be empty." });

        var item = new TodoItem { Title = request.Title.Trim() };
        var created = _repository.Add(item);

        _logger.LogInformation("Created todo {Id}: {Title}", created.Id, created.Title);

        return CreatedAtRoute(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    /// <summary>Deletes a todo item by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var deleted = _repository.Delete(id);
        if (!deleted)
        {
            _logger.LogWarning("Attempted to delete non-existent todo {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Deleted todo {Id}", id);
        return NoContent();
    }

    /// <summary>Toggles the completion status of a todo item.</summary>
    [HttpPatch("{id:guid}/toggle")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Toggle(Guid id)
    {
        var item = _repository.GetById(id);
        if (item is null) return NotFound();

        item.IsCompleted = !item.IsCompleted;
        var updated = _repository.Update(item)!;

        return Ok(MapToResponse(updated));
    }

    private static TodoResponse MapToResponse(TodoItem item)
        => new(item.Id, item.Title, item.IsCompleted, item.CreatedAt);
}
