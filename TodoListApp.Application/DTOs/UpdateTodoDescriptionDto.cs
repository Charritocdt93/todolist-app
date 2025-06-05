namespace TodoListApp.Application.DTOs
{
    /// <summary>
    /// DTO para actualizar solo la descripción de un TodoItem existente.
    /// </summary>
    public class UpdateTodoDescriptionDto
    {
        public string NewDescription { get; set; } = string.Empty;
    }
}
