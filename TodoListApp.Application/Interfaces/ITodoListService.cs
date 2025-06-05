using TodoListApp.Application.DTOs;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Interfaces
{
    /// <summary>
    /// Interfaz que define los casos de uso (servicios de aplicación) para gestionar TodoItems.
    /// </summary>
    public interface ITodoListService
    {
        /// <summary>
        /// Crea un nuevo TodoItem y devuelve su Id generado.
        /// </summary>
        /// <param name="title">Título del ítem.</param>
        /// <param name="description">Descripción.</param>
        /// <param name="category">Categoría (debe ser válida).</param>
        /// <returns>Id del nuevo ítem.</returns>
        int CreateItem(string title, string description, string category);

        /// <summary>
        /// Actualiza la descripción de un TodoItem existente.
        /// </summary>
        void UpdateItemDescription(int id, string newDescription);

        /// <summary>
        /// Elimina un TodoItem (solo si no supera 50% completado).
        /// </summary>
        void RemoveItem(int id);

        /// <summary>
        /// Registra una nueva progresión en un TodoItem.
        /// </summary>
        void RegisterProgression(int id, DateTime date, decimal percent);

        /// <summary>
        /// Devuelve todos los TodoItems con sus progresiones (mapeados a DTO).
        /// </summary>
        IEnumerable<TodoItem> GetAllItems();

        /// <summary>
        /// Devuelve un TodoItems segun su id con sus progresiones (mapeados a DTO).
        /// </summary>
        TodoItem? GetById(int id);
    }
}
