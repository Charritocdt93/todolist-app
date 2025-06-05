namespace TodoListApp.Application.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo TodoItem.
    /// Contiene solo los campos mínimos requeridos desde la capa de presentación.
    /// </summary>
    public class CreateTodoItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
