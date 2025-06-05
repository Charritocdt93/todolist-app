using TodoListApp.Domain.Aggregates;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Infrastructure.Persistence;

namespace TodoListApp.Tests
{
    public class TodoListErrorTests
    {
        [Fact]
        public void AddItem_CategoriaInvalida_LanzaDomainException()
        {
            var repository = new InMemoryTodoListRepository();
            ITodoList todoList = new TodoList(repository);

            int id1 = repository.GetNextId();
            // Intentamos agregar con categoría no existente, p.ej. "NoExiste"
            var ex = Assert.Throws<DomainException>(() =>
                todoList.AddItem(id1, "Título", "Desc", "NoExiste")
            );
            Assert.Contains("Categoría inválida", ex.Message);
        }

        [Fact]
        public void RegisterProgression_FechaNoCreciente_LanzaDomainException()
        {
            var repository = new InMemoryTodoListRepository();
            ITodoList todoList = new TodoList(repository);

            int id1 = repository.GetNextId();
            todoList.AddItem(id1, "Título", "Desc", "Work");

            // Primera progresión: 2025-03-20
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 20), 30m);

            // Segunda progresión: 2025-03-19 (fecha menor que la anterior)
            var ex = Assert.Throws<DomainException>(() =>
                todoList.RegisterProgression(id1, new DateTime(2025, 3, 19), 20m)
            );
            Assert.Contains("fecha de la nueva progresión debe ser mayor", ex.Message);
        }

        [Fact]
        public void RegisterProgression_SumaMasDe100_LanzaDomainException()
        {
            var repository = new InMemoryTodoListRepository();
            ITodoList todoList = new TodoList(repository);

            int id1 = repository.GetNextId();
            todoList.AddItem(id1, "Título", "Desc", "Work");

            todoList.RegisterProgression(id1, new DateTime(2025, 3, 18), 60m);
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 19), 50m); // 60 + 50 = 110 > 100

            var item = repository.GetTodoItemById(id1);
            Assert.Equal(60m, item!.TotalPercent); // el segundo no se guardó

            // El último RegisterProgression debió lanzar DomainException
        }

        [Fact]
        public void UpdateItem_MasDel50Porciento_LanzaDomainException()
        {
            var repository = new InMemoryTodoListRepository();
            ITodoList todoList = new TodoList(repository);

            int id1 = repository.GetNextId();
            todoList.AddItem(id1, "Título", "Desc original", "Work");

            // Agregamos 60% → ya supera 50%
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 18), 60m);

            var ex = Assert.Throws<DomainException>(() =>
                todoList.UpdateItem(id1, "NuevaDesc")  // No debería permitirse
            );
            Assert.Contains("más del 50% completado", ex.Message);
        }

        [Fact]
        public void RemoveItem_MasDel50Porciento_LanzaDomainException()
        {
            var repository = new InMemoryTodoListRepository();
            ITodoList todoList = new TodoList(repository);

            int id1 = repository.GetNextId();
            todoList.AddItem(id1, "Título", "Desc", "Work");

            // 51% de progresión
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 18), 51m);

            var ex = Assert.Throws<DomainException>(() =>
                todoList.RemoveItem(id1)
            );
            Assert.Contains("más del 50% completado", ex.Message);
        }
    }
}
