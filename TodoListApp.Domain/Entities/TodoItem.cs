using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un elemento de TODO con sus progresiones.
    /// </summary>
    public class TodoItem
    {
        public int Id { get; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string Category { get; }

        private readonly List<Progression> _progressions = new();

        public IReadOnlyList<Progression> Progressions => _progressions.AsReadOnly();

        /// <summary>
        /// Suma de todos los porcentajes en la lista de progresiones.
        /// </summary>
        public decimal TotalPercent => _progressions.Sum(p => p.Percent);

        /// <summary>
        /// True si TotalPercent == 100m.
        /// </summary>
        public bool IsCompleted => TotalPercent == 100m;

        /// <summary>
        /// Constructor: crea un nuevo TodoItem (sin progresiones iniciales).
        /// </summary>
        public TodoItem(int id, string title, string description, string category)
        {
            if (id <= 0)
                throw new DomainException("Id debe ser mayor que cero.");
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Title no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("Description no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(category))
                throw new DomainException("Category no puede estar vacío.");

            Id = id;
            Title = title.Trim();
            Description = description.Trim();
            Category = category.Trim();
        }

        /// <summary>
        /// Actualiza la descripción. Solo permitido si TotalPercent ≤ 50.
        /// </summary>
        public void UpdateDescription(string nuevaDescription)
        {
            if (TotalPercent > 50m)
                throw new DomainException("No se puede actualizar un TodoItem con más del 50% completado.");

            if (string.IsNullOrWhiteSpace(nuevaDescription))
                throw new DomainException("Description no puede estar vacío.");

            Description = nuevaDescription.Trim();
        }

        /// <summary>
        /// Registra una nueva progresión en este TodoItem:
        /// - La fecha debe ser mayor que la última existente (si hubiera).
        /// - El porcentaje no debe hacer que TotalPercent supere 100.
        /// - El objeto Progression en sí valida 0 < percent ≤ 100.
        /// </summary>
        public void AddProgression(Progression nuevaProg)
        {
            if (nuevaProg == null)
                throw new DomainException("Progression no puede ser nula.");

            // Validamos fecha creciente
            if (_progressions.Any())
            {
                DateTime fechaMaxima = _progressions.Max(p => p.Date);
                if (nuevaProg.Date <= fechaMaxima)
                    throw new DomainException("La fecha de la nueva progresión debe ser mayor que las existentes.");
            }

            // Validamos que no exceda 100
            if (TotalPercent + nuevaProg.Percent > 100m)
                throw new DomainException("La suma total de porcentajes no puede exceder 100%.");

            _progressions.Add(nuevaProg);
        }
    }
}
