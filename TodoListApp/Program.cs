using TodoListApp.Domain.Aggregates;
using TodoListApp.Infrastructure.Persistence;

namespace TodoListApp.ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1) Creamos el repositorio en memoria
            var repository = new InMemoryTodoListRepository();

            // 2) Creamos el agregado TodoList, inyectando el repositorio
            ITodoList todoList = new Domain.Aggregates.TodoList(repository);

            // ===================================================================
            // Ejemplo fijo (happy path) tal como pide el enunciado:
            // ===================================================================
            //  - Id=1
            //  - Title="Complete Project Report"
            //  - Description="Finish the final report for the project"
            //  - Category="Work"
            //  - Tres progresiones: (2025-03-18, 30), (2025-03-19, 50), (2025-03-20, 20)
            // ===================================================================
            int id1 = repository.GetNextId();
            todoList.AddItem(id1,
                             "Complete Project Report",
                             "Finish the final report for the project",
                             "Work");

            // Registramos las tres progresiones:
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 18), 30m);
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 19), 50m);
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 20), 20m);

            // Ahora imprimimos los items:
            todoList.PrintItems();

            // Fin de ejemplo. Si quisiéramos, aquí podríamos pedir un bucle con menú
            // para que el usuario interactivamente agregue, actualice, elimine.
        }
    }
}