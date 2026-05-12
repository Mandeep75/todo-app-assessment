using System.Collections.Concurrent;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Thread-safe in-memory todo store backed by a ConcurrentDictionary.
/// Registered as Singleton so state persists for the lifetime of the process.
/// </summary>
public class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _store = new();

    public IEnumerable<TodoItem> GetAll()
        => _store.Values.OrderBy(t => t.CreatedAt);

    public TodoItem? GetById(Guid id)
        => _store.TryGetValue(id, out var item) ? item : null;

    public TodoItem Add(TodoItem item)
    {
        _store[item.Id] = item;
        return item;
    }

    public bool Delete(Guid id)
        => _store.TryRemove(id, out _);

    public TodoItem? Update(TodoItem item)
    {
        if (!_store.ContainsKey(item.Id))
            return null;

        _store[item.Id] = item;
        return item;
    }
}
