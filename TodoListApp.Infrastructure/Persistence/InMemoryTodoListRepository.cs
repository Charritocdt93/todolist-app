using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Repositories;

namespace TodoListApp.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación en memoria de ITodoListRepository. No usa una base de datos real.
    /// </summary>
    public class InMemoryTodoListRepository : ITodoListRepository
    {
        private readonly ConcurrentDictionary<int, TodoItem> _store = new();
        private readonly ILogger<InMemoryTodoListRepository> _logger;
        private int _nextId = 0;

        public InMemoryTodoListRepository(ILogger<InMemoryTodoListRepository> logger)
        {
            _logger = logger;
            _logger.LogDebug("InMemoryTodoListRepository inicializado.");
        }

        public InMemoryTodoListRepository()
            : this(NullLogger<InMemoryTodoListRepository>.Instance)
        {
            // Aquí no necesitamos más; simplemente delegamos al constructor principal.
        }

        public int GetNextId()
        {
            var id = Interlocked.Increment(ref _nextId);
            _logger.LogDebug("GetNextId() -> {Id}", id);
            return id;
        }

        public List<string> GetAllCategories()
        {
            var cats = new List<string> { "Work", "Personal", "Hobby", "Other" };
            _logger.LogDebug("GetAllCategories() -> {Cats}", string.Join(",", cats));
            return cats;
        }

        public void AddTodoItem(TodoItem item)
        {
            if (!_store.TryAdd(item.Id, item))
            {
                _logger.LogError("AddTodoItem falló: Id duplicado {Id}", item.Id);
                throw new InvalidOperationException($"Id duplicado: {item.Id}");
            }
            _logger.LogInformation("TodoItem agregado en memoria con Id={Id}", item.Id);
        }

        public TodoItem GetTodoItemById(int id)
        {
            _store.TryGetValue(id, out var item);
            if (item != null)
                _logger.LogDebug("GetTodoItemById({Id}) -> encontrado", id);
            else
                _logger.LogDebug("GetTodoItemById({Id}) -> no encontrado", id);
            return item;
        }

        public IEnumerable<TodoItem> GetAllTodoItems()
        {
            _logger.LogDebug("GetAllTodoItems() -> count={Count}", _store.Count);
            return _store.Values.ToList();
        }

        public void RemoveTodoItem(int id)
        {
            if (_store.TryRemove(id, out _))
            {
                _logger.LogInformation("TodoItem Id={Id} eliminado de memoria", id);
            }
            else
            {
                _logger.LogWarning("RemoveTodoItem({Id}) falló: no existe", id);
                throw new KeyNotFoundException($"No existe TodoItem con Id={id}");
            }
        }
    }
}
