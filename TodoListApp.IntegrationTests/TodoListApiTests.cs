using System.Net;
using System.Net.Http.Json;

namespace TodoListApp.IntegrationTests
{
    public class TodoListApiTests :
        IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TodoListApiTests(WebApplicationFactory<Program> factory)
        {
            // Crea un cliente HTTP apuntando a la API en memoria
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_TodoList_ReturnsEmptyArray_WhenNoItemsExist()
        {
            // Act
            var response = await _client.GetAsync("/api/todolist");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await response.Content.ReadFromJsonAsync<object[]>();
            list.Should().NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public async Task Post_TodoList_CreatesNewItem_AndCanRetrieveIt()
        {
            // Arrange: payload JSON
            var newTodo = new
            {
                title = "IntegrateTest",
                description = "Descripción desde integración",
                category = "Work"
            };

            // Act: creamos un nuevo to-do
            var postResponse = await _client.PostAsJsonAsync("/api/todolist", newTodo);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Opcional: leer Location o Body
            var created = await postResponse.Content.ReadFromJsonAsync<TodoItemDto>();
            created.Should().NotBeNull();
            created!.Title.Should().Be("IntegrateTest");
            created.Id.Should().BePositive();

            // Act: recuperamos con GET/{id}
            var getResponse = await _client.GetAsync($"/api/todolist/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var fetched = await getResponse.Content.ReadFromJsonAsync<TodoItemDto>();
            fetched.Should().NotBeNull();
            fetched!.Id.Should().Be(created.Id);
            fetched.Title.Should().Be(created.Title);
        }

        // DTO interno que coincide con el que tu API devuelve
        private class TodoItemDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = default!;
            public string Description { get; set; } = default!;
            public string Category { get; set; } = default!;
            public bool IsCompleted { get; set; }
            // Puedes añadir Progressions y TotalPercent si lo deseas
        }
    }
}
