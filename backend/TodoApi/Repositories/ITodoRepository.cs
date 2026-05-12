using TodoApi.Models;

namespace TodoApi.Repositories;

public interface ITodoRepository
{
    IEnumerable<TodoItem> GetAll();
    TodoItem? GetById(Guid id);
    TodoItem Add(TodoItem item);
    bool Delete(Guid id);
    TodoItem? Update(TodoItem item);
}
