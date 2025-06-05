namespace TodoListApp.Application.DTOs
{
    /// <summary>
    /// DTO para registrar una progresión (avance parcial) en un TodoItem.
    /// </summary>
    public class RegisterProgressionDto
    {
        public DateTime Date { get; set; }
        public decimal Percent { get; set; }
    }
}
