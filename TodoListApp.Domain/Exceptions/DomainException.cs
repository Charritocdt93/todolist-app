namespace TodoListApp.Domain.Exceptions
{
    /// <summary>
    /// Excepción genérica para violaciones de las reglas de dominio.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string mensaje)
            : base(mensaje)
        { }
    }
}
