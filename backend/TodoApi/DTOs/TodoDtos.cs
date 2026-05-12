namespace TodoApi.DTOs;

/// <summary>Request body for creating a new todo item.</summary>
public record CreateTodoRequest(string Title);

/// <summary>API response shape — keeps domain model internal.</summary>
public record TodoResponse(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAt
);
