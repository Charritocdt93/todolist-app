using Microsoft.OpenApi.Models;
using Serilog;
using TodoListApp.Api.Middleware;
using TodoListApp.Application.Interfaces;
using TodoListApp.Application.Services;
using TodoListApp.Domain.Repositories;
using TodoListApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1) Configurar Serilog desde appsettings.json
// --------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)  // lee la sección "Serilog" de appsettings.json
    .Enrich.FromLogContext()
    .CreateLogger();

// Decimos a ASP.NET Core que use Serilog para todos los logs
builder.Host.UseSerilog();

// 2) Indicamos a ASP.NET Core que use Serilog como proveedor de logging
builder.Host.UseSerilog();

// (a) Registramos controladores MVC (REST)
builder.Services.AddControllers();

// (b) Registramos Swagger/OpenAPI para documentar los endpoints REST
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TodoList API",
        Version = "v1",
        Description = "API REST para gestionar TodoItems"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostSwagger", policy =>
    {
        policy
          .WithOrigins("http://localhost:5284") // o el puerto/punto donde esté corriendo tu Swagger UI
          .AllowAnyMethod()
          .AllowAnyHeader();
    });
});

// (c) Registramos repositorio e implementación de la capa de aplicación (casos de uso)
builder.Services.AddSingleton<ITodoListRepository, InMemoryTodoListRepository>();
builder.Services.AddScoped<ITodoListService, TodoListService>();

var app = builder.Build();

// --------------------------------------------------
// 3) Integrar el middleware de excepciones (antes de cualquier otro middleware que pueda lanzar error)
// --------------------------------------------------
app.UseCustomExceptionHandling();

// --------------------------------------------------
// 2) Configurar pipeline HTTP / Middlewares
// --------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // (a) Habilitar Swagger UI solamente en entorno Development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList API v1");
    });
}

//// (b) Forzamos redirección HTTP → HTTPS
//app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowLocalhostSwagger");
app.MapControllers();

// --------------------------------------------------
// 4) Servir front-end estático desde wwwroot (opcional)
// --------------------------------------------------
app.UseDefaultFiles(); // Busca y sirve por defecto wwwroot/index.html
app.UseStaticFiles();  // Sirve CSS, JS, imágenes, etc., que estén en wwwroot/

app.Run();
