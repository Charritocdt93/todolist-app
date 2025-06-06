using Microsoft.Extensions.Logging;
using TodoListApp.Application.Interfaces;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.Repositories;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Services
{
    /// <summary>
    /// Implementación de los casos de uso para gestionar TodoItems.
    /// Orquesta las operaciones de dominio y repositorio, y mapea a/fro los DTOs.
    /// </summary>
    public class TodoListService : ITodoListService
    {
        private readonly ITodoListRepository _repository;
        private readonly ILogger<TodoListService> _logger;

        public TodoListService(ITodoListRepository repository, ILogger<TodoListService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int CreateItem(string title, string description, string category)
        {
            _logger.LogInformation("Creando TodoItem: Title={Title}, Category={Category}", title, category);

            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("El Título no puede estar vacío.");

            // Intentamos parsear el string a Category enum
            if (!Enum.TryParse<Category>(category, ignoreCase: true, out var categoryEnum))
                throw new ArgumentException($"Categoría inválida: {category}");

            var nextId = _repository.GetNextId();
            var categorias = _repository.GetAllCategories();
            if (!categorias.Contains(category))
                throw new DomainException($"Categoría inválida: {category}");

            var item = new TodoItem(nextId, title, description, category);
            _repository.AddTodoItem(item);

            _logger.LogInformation("TodoItem creado con Id={Id}", nextId);
            return nextId;
        }

        public IEnumerable<TodoItem> GetAllItems()
        {
            _logger.LogInformation("Obteniendo todos los TodoItems");
            return _repository.GetAllTodoItems();
        }

        public TodoItem? GetById(int id)
        {
            _logger.LogInformation("Obteniendo TodoItem Id={Id}", id);
            return _repository.GetTodoItemById(id);
        }

        public void UpdateItemDescription(int id, string newDescription)
        {
            _logger.LogInformation("Actualizando descripción de TodoItem Id={Id}", id);

            var item = _repository.GetTodoItemById(id)
                       ?? throw new DomainException($"No existe TodoItem con Id={id}");

            if (item.TotalPercent > 50m)
                throw new DomainException("No se puede actualizar un TodoItem con más del 50% completado.");

            item.UpdateDescription(newDescription);
            _logger.LogInformation("Descripción del TodoItem Id={Id} actualizada", id);
        }

        public void RemoveItem(int id)
        {
            _logger.LogInformation("Eliminando TodoItem Id={Id}", id);

            var item = _repository.GetTodoItemById(id)
                       ?? throw new DomainException($"No existe TodoItem con Id={id}");

            if (item.TotalPercent > 50m)
                throw new DomainException("No se puede eliminar un TodoItem con más del 50% completado.");

            _repository.RemoveTodoItem(id);
            _logger.LogInformation("TodoItem Id={Id} eliminado", id);
        }

        public void RegisterProgression(int id, DateTime dateTime, decimal percent)
        {
            _logger.LogInformation("Registrando progresión para TodoItem Id={Id}, Fecha={Date}, Percent={Percent}", id, dateTime, percent);

            var item = _repository.GetTodoItemById(id)
                       ?? throw new DomainException($"No existe TodoItem con Id={id}");

            var prog = new Progression(dateTime, percent);
            item.AddProgression(prog);

            _logger.LogInformation("Progresión registrada en TodoItem Id={Id}: Fecha={Date}, Percent={Percent}", id, dateTime, percent);
        }
    }
}
