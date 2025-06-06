using Microsoft.AspNetCore.Mvc;
using TodoListApp.Application.DTOs;
using TodoListApp.Application.Interfaces;

namespace TodoListApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoListController : ControllerBase
    {
        private readonly ITodoListService _todoService;
        private readonly ILogger<TodoListController> _logger;

        public TodoListController(ITodoListService todoService, ILogger<TodoListController> logger)
        {
            _todoService = todoService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GET /api/todolist solicitados.");
            var items = _todoService.GetAllItems();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation("GET /api/todolist/{Id} solicitado.", id);
            var item = _todoService.GetById(id);
            if (item == null)
            {
                _logger.LogWarning("No se encontró TodoItem Id={Id}", id);
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateTodoItemDto dto)
        {
            _logger.LogInformation("POST /api/todolist - Título={Title}, Categoría={Category}", dto.Title, dto.Category);
            // Si ocurre DomainException, será capturada por el middleware
            var newId = _todoService.CreateItem(dto.Title, dto.Description, dto.Category);
            return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
        }

        [HttpPut("{id:int}")]
        public IActionResult UpdateDescription(int id, [FromBody] UpdateTodoDescriptionDto dto)
        {
            _logger.LogInformation("PUT /api/todolist/{Id} - NuevaDesc={Desc}", id, dto.NewDescription);
            _todoService.UpdateItemDescription(id, dto.NewDescription);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("DELETE /api/todolist/{Id}", id);
            _todoService.RemoveItem(id);
            return NoContent();
        }

        [HttpPost("{id:int}/progressions")]
        public IActionResult RegisterProgression(int id, [FromBody] RegisterProgressionDto dto)
        {
            _logger.LogInformation("POST /api/todolist/{Id}/progressions - Fecha={Date}, Percent={Percent}", id, dto.Date, dto.Percent);
            _todoService.RegisterProgression(id, dto.Date, dto.Percent);
            return NoContent();
        }
    }
}
