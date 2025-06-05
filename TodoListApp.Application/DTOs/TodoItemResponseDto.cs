namespace TodoListApp.Application.DTOs
{
    /// <summary>
    /// DTO de respuesta que encapsula los datos de un TodoItem, incluyendo sus progresiones.
    /// </summary>
    public class TodoItemResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Lista de progresiones con fecha, porcentaje individual y porcentaje acumulado.
        /// </summary>
        public List<ProgressionResponseDto> Progressions { get; set; } = new();
    }

    public class ProgressionResponseDto
    {
        public DateTime Date { get; set; }
        public decimal Percent { get; set; }
        public decimal AccumulatedPercent { get; set; }
    }
}
