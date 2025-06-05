using TodoListApp.Domain.Aggregates;
using TodoListApp.Infrastructure.Persistence;

namespace TodoListApp.Tests
{
    public class TodoListServiceTests
    {
        [Fact]
        public void HappyPath_EjemploDelEnunciado_SeImprimeCorrectamente()
        {
            // 1) Arrange
            var repository = new InMemoryTodoListRepository();
            ITodoList todoList = new TodoList(repository);

            int id1 = repository.GetNextId();
            // 2) Act
            todoList.AddItem(id1,
                             "Complete Project Report",
                             "Finish the final report for the project",
                             "Work");
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 18), 30m);
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 19), 50m);
            todoList.RegisterProgression(id1, new DateTime(2025, 3, 20), 20m);

            // Para comprobar que realmente quedó con total 100 %
            var item = repository.GetTodoItemById(id1);
            Assert.Equal(100m, item!.TotalPercent);
            Assert.True(item.IsCompleted);

            // 3) Imprimimos a un writer que podamos leer (capturamos la salida de consola):
            using var consoleOutput = new ConsoleOutputCapture();
            todoList.PrintItems();  // esto escribe en Consola

            string salida = consoleOutput.GetOutput();

            // 4) Assert: Verificar que la salida contenga las líneas clave:
            Assert.Contains("1) Complete Project Report - Finish the final report for the project (Work) Completed:True", salida);
            Assert.Contains("3/18/2025", salida);
            Assert.Contains("30%", salida);
            Assert.Contains("3/19/2025", salida);
            Assert.Contains("80%", salida);   // 30+50 = 80 acumulado
            Assert.Contains("3/20/2025", salida);
            Assert.Contains("100%", salida);  // 30+50+20 = 100 acumulado
        }
    }

    /// <summary>
    /// Clase auxiliar para capturar la salida de la consola durante un test.
    /// </summary>
    internal class ConsoleOutputCapture : IDisposable
    {
        private readonly StringWriter _stringWriter;
        private readonly TextWriter _originalOutput;

        public ConsoleOutputCapture()
        {
            _stringWriter = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_stringWriter);
        }

        public string GetOutput()
        {
            return _stringWriter.ToString();
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _stringWriter.Dispose();
        }
    }
}
