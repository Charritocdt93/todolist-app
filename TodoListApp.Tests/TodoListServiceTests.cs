using Microsoft.Extensions.Logging.Abstractions;
using TodoListApp.Application.Services;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Persistence;

namespace TodoListApp.Tests
{
    public class TodoListServiceTests
    {

        #region CreateItem

        [Fact]
        public void CreateTodoItem_ConDatosValidos_RetornaItemConPropiedadesCorrectas()
        {
            // 1) Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Datos de entrada
            string title = "Tarea 1";
            string description = "Prueba unitaria inicial";
            string category = "Work";

            // 2) Act: invocamos CreateItem
            int newId = service.CreateItem(title, description, category);

            // 3) Assert: 
            //    a) El método devolvió el id esperado
            Assert.Equal(1, newId);

            //    b) El repositorio ahora contiene un TodoItem con ese Id
            var item = service.GetById(newId);
            Assert.NotNull(item);

            //    c) Todas sus propiedades coinciden
            Assert.Equal(1, item.Id);
            Assert.Equal(title, item.Title);
            Assert.Equal(description, item.Description);
            Assert.Equal(category, item.Category);
            Assert.False(item.IsCompleted);
            Assert.Empty(item.Progressions);
            Assert.Equal(0m, item.TotalPercent);
        }

        [Fact]
        public void CreateItem_ConTituloVacio_LanzaDomainException()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() =>
                service.CreateItem(title: "", description: "Desc", category: "Work"));
            Assert.Contains("Título no puede estar vacío", ex.Message);
        }

        [Fact]
        public void CreateItem_ConCategoriaInvalida_LanzaArgumentException()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                service.CreateItem(title: "Tarea", description: "Desc", category: "NoExiste"));
            Assert.Contains("Categoría inválida", ex.Message);
        }

        #endregion

        # region RegisterProgression

        [Fact]
        public void RegisterProgression_ConProgresionesValidas_AcumulaTotalPercentYMarcaCompletedAl100()
        {
            // Arrange: servicio listo con logger y repo limpio
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Primero creamos el ToDo
            int id = service.CreateItem("TareaProg", "Test progresión", "Work");

            // Fechas ordenadas
            var fecha1 = DateTime.UtcNow.AddMinutes(1);
            var fecha2 = fecha1.AddMinutes(1);
            var fecha3 = fecha2.AddMinutes(1);

            // Act: registramos tres progresiones válidas
            service.RegisterProgression(id, fecha1, 10m);
            service.RegisterProgression(id, fecha2, 20m);

            // Comprobamos intermedio
            var todo = service.GetById(id)!;
            Assert.Equal(2, todo.Progressions.Count());
            Assert.Equal(30m, todo.TotalPercent);
            Assert.False(todo.IsCompleted);

            // Ahora llegamos al 100%
            service.RegisterProgression(id, fecha3, 70m);

            // Assert final
            Assert.Equal(3, todo.Progressions.Count());
            Assert.Equal(100m, todo.TotalPercent);
            Assert.True(todo.IsCompleted);
        }

        [Fact]
        public void RegisterProgression_SiItemNoExiste_LanzaDomainException()
        {
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Act & Assert: id 999 no existe
            var fecha = DateTime.UtcNow;
            var ex = Assert.Throws<DomainException>(() =>
                service.RegisterProgression(999, fecha, 10m));
            Assert.Contains("No existe TodoItem", ex.Message);
        }

        [Fact]
        public void RegisterProgression_FechaNoMayorALaAnterior_LanzaDomainException()
        {
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Crear item y primera progresión
            int id = service.CreateItem("TareaErrFecha", "Desc", "Work");
            var fecha = DateTime.UtcNow.AddMinutes(1);
            service.RegisterProgression(id, fecha, 10m);

            // Act & Assert: intento con fecha igual o anterior
            var ex = Assert.Throws<DomainException>(() =>
                service.RegisterProgression(id, fecha.AddSeconds(-10), 20m));
            Assert.Contains("fecha de la nueva progresión debe ser mayor", ex.Message);
        }

        [Fact]
        public void RegisterProgression_PorcentajeNoMayorAlAnterior_LanzaDomainException()
        {
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Crear item y primera progresión
            int id = service.CreateItem("TareaErrPercent", "Desc", "Work");
            var fecha1 = DateTime.UtcNow.AddMinutes(1);
            service.RegisterProgression(id, fecha1, 50m);

            // Act & Assert: intento con porcentaje igual o menor
            var fecha2 = fecha1.AddMinutes(1);
            var ex = Assert.Throws<DomainException>(() =>
                service.RegisterProgression(id, fecha2, 50m));
            Assert.Contains("El porcentaje de la nueva progresión debe ser mayor que el anterior.", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region UpdateItemDescription

        [Fact]
        public void UpdateItemDescription_ConIdValido_YDescripcionCorrecta_CambiaLaDescripcion()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Creamos un ToDo
            int id = service.CreateItem("Tarea", "Descripción inicial", "Work");

            // Act: actualizamos la descripción
            string nuevaDesc = "Descripción actualizada";
            service.UpdateItemDescription(id, nuevaDesc);

            // Assert: verificamos que haya cambiado
            var todo = service.GetById(id)!;
            Assert.Equal(nuevaDesc, todo.Description);
        }

        [Fact]
        public void UpdateItemDescription_IdInexistente_LanzaDomainException()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() =>
                service.UpdateItemDescription(999, "Cualquier cosa"));
            Assert.Contains("No existe TodoItem con Id=999", ex.Message);
        }

        [Fact]
        public void UpdateItemDescription_ProgresoMayor50_LanzaDomainException()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Creamos y le registramos >50% de progreso
            int id = service.CreateItem("Tarea", "Desc", "Work");
            var fecha = DateTime.UtcNow.AddMinutes(1);
            service.RegisterProgression(id, fecha, 60m);  // TotalPercent = 60

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() =>
                service.UpdateItemDescription(id, "Nueva descripción"));
            Assert.Contains(
                "No se puede actualizar un TodoItem con más del 50% completado",
                ex.Message);
        }

        #endregion

        #region RemoveItem

        [Fact]
        public void RemoveItem_ConProgresoMenorOIgual50_EliminaCorrectamente()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Creamos un ToDo y le damos un progreso del 50%
            int id = service.CreateItem("TareaParaBorrar", "Prueba delete", "Work");
            var fecha = DateTime.UtcNow.AddMinutes(1);
            service.RegisterProgression(id, fecha, 50m);

            // Act
            service.RemoveItem(id);

            // Assert: tras eliminar, GetById debe devolver null
            var deleted = service.GetById(id);
            Assert.Null(deleted);
        }

        [Fact]
        public void RemoveItem_IdInexistente_LanzaDomainException()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() =>
                service.RemoveItem(999));
            Assert.Contains("No existe TodoItem con Id=999", ex.Message);
        }

        [Fact]
        public void RemoveItem_ConProgresoMayor50_LanzaDomainException()
        {
            // Arrange
            var repo = new InMemoryTodoListRepository();
            var logger = NullLogger<TodoListService>.Instance;
            var service = new TodoListService(repo, logger);

            // Creamos y registramos progreso > 50%
            int id = service.CreateItem("TareaBloqueada", "No se debe borrar", "Work");
            var fecha1 = DateTime.UtcNow.AddMinutes(1);
            service.RegisterProgression(id, fecha1, 60m);

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() =>
                service.RemoveItem(id));
            Assert.Contains(
                "No se puede eliminar un TodoItem con más del 50% completado",
                ex.Message);
        }

        #endregion
    }
}
