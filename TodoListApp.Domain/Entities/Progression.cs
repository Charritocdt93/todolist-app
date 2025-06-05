using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Entities
{
    /// <summary>
    /// Representa una única progresión (avance parcial) dentro de un TodoItem.
    /// </summary>
    public class Progression
    {
        /// <summary>Fecha en la que se registró la progresión.</summary>
        public DateTime Date { get; }

        /// <summary>Porcentaje que representa esta progresión (del 0 al 100).</summary>
        public decimal Percent { get; }

        public Progression(DateTime date, decimal percent)
        {
            if (percent <= 0m || percent > 100m)
                throw new DomainException("Percent debe ser mayor que 0 y menor o igual a 100.");

            Date = date;
            Percent = percent;
        }
    }
}
