namespace TodoListApp.Domain.Aggregates
{
    /// <summary>
    /// Interfaz pública que define las operaciones del agregado TodoList.
    /// </summary>
    public interface ITodoList
    {
        void AddItem(int id, string title, string description, string category);
        void UpdateItem(int id, string description);
        void RemoveItem(int id);
        void RegisterProgression(int id, DateTime dateTime, decimal percent);
        void PrintItems();
    }
}
