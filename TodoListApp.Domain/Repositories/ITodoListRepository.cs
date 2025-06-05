using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Repositories
{
    /// <summary>
    /// Interfaz de repositorio que provee el siguiente Id y las categorías válidas.
    /// Además, nosotros añadiremos la posibilidad de almacenar y recuperar TodoItems.
    /// </summary>
    public interface ITodoListRepository
    {
        /// <summary>
        /// Devuelve el siguiente Id disponible para crear un TodoItem.
        /// </summary>
        int GetNextId();

        /// <summary>
        /// Devuelve la lista de categorías válidas del sistema.
        /// </summary>
        List<string> GetAllCategories();

        /// <summary>
        /// Agrega un TodoItem al repositorio (persistencia).
        /// </summary>
        void AddTodoItem(TodoItem item);

        /// <summary>
        /// Recupera un TodoItem por su Id. Si no existe, devuelve null.
        /// </summary>
        TodoItem? GetTodoItemById(int id);

        /// <summary>
        /// Elimina el TodoItem del repositorio (persistencia).
        /// </summary>
        void RemoveTodoItem(int id);

        /// <summary>
        /// Devuelve todos los TodoItems almacenados.
        /// </summary>
        IEnumerable<TodoItem> GetAllTodoItems();
    }
}
