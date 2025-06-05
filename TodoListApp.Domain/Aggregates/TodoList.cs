using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.Repositories;
using TodoListApp.Domain.Utils;

namespace TodoListApp.Domain.Aggregates
{
    /// <summary>
    /// Agregado raíz que gestiona la colección completa de TodoItems
    /// consultando directamente al repositorio en cada operación.
    /// </summary>
    public class TodoList : ITodoList
    {
        private readonly ITodoListRepository _repository;

        public TodoList(ITodoListRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void AddItem(int id, string title, string description, string category)
        {
            // 1) Validar categoría
            var categoriasValidas = _repository.GetAllCategories();
            if (!categoriasValidas.Contains(category))
                throw new DomainException("Categoría inválida.");

            // 2) Verificar que el id no exista ya en el repositorio
            var existe = _repository.GetTodoItemById(id);
            if (existe != null)
                throw new DomainException($"Ya existe un TodoItem con Id = {id}.");

            // 3) Crear la entidad
            var item = new TodoItem(id, title, description, category);

            // 4) Persistir en el repositorio (in memory)
            _repository.AddTodoItem(item);
        }

        public void UpdateItem(int id, string newDescription)
        {
            // 1) Obtener el item desde el repositorio
            var item = _repository.GetTodoItemById(id) ?? throw new DomainException("El TodoItem no existe.");

            // 2) Ejecutar la lógica de negocio (validaciones internas)
            item.UpdateDescription(newDescription);

            // 3) No hace falta un “Update” explícito porque
            //    el repositorio in-memory mantiene la misma instancia.
        }

        public void RemoveItem(int id)
        {
            // 1) Obtener el item
            var item = _repository.GetTodoItemById(id) ?? throw new DomainException("El TodoItem no existe.");

            // 2) Validar progreso ≤ 50%
            if (item.TotalPercent > 50m)
                throw new DomainException("No se puede eliminar un TodoItem con más del 50% completado.");

            // 3) Eliminarlo del repositorio
            _repository.RemoveTodoItem(id);
        }

        public void RegisterProgression(int id, DateTime dateTime, decimal percent)
        {
            // 1) Obtener el item
            var item = _repository.GetTodoItemById(id) ?? throw new DomainException("El TodoItem no existe.");

            // 2) Crear la progresión con validación interna en Progression
            var nuevaProg = new Progression(dateTime, percent);

            // 3) Agregar la progresión a la entidad
            item.AddProgression(nuevaProg);

            // 4) No se requiere persistir manualmente porque el repositorio guarda la misma instancia
        }

        public void PrintItems()
        {
            // 1) Pedir todos los items al repositorio y ordenarlos
            var todos = _repository.GetAllTodoItems()
                       .OrderBy(x => x.Id)
                       .ToList();

            // 2) Imprimir cada uno
            foreach (var item in todos)
            {
                Console.WriteLine(
                    $"{item.Id}) {item.Title} - {item.Description} ({item.Category}) Completed:{item.IsCompleted}");

                decimal acumulado = 0m;
                foreach (var prog in item.Progressions)
                {
                    acumulado += prog.Percent;
                    string barra = ProgressBarRenderer.Render(acumulado);
                    Console.WriteLine($"{prog.Date} - {acumulado}% {barra}");
                }
            }
        }
    }
}
