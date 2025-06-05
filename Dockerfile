################################################################################
# ETAPA 1: BUILD + PUBLISH con .NET 8 SDK
################################################################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# -------------------------------------------------------------------------------
# 1) Copiamos únicamente los archivos .csproj de los proyectos que SÍ queremos
#    (Api + Application + Domain + Infrastructure). De este modo, Docker podrá
#    usar la cache si no cambian estos archivos, y no arrastramos el proyecto
#    de consola ni los tests en esta etapa.
# -------------------------------------------------------------------------------
COPY TodoListApp.Api/TodoListApp.Api.csproj                  TodoListApp.Api/
COPY TodoListApp.Application/TodoListApp.Application.csproj  TodoListApp.Application/
COPY TodoListApp.Domain/TodoListApp.Domain.csproj            TodoListApp.Domain/
COPY TodoListApp.Infrastructure/TodoListApp.Infrastructure.csproj  TodoListApp.Infrastructure/

# -------------------------------------------------------------------------------
# 2) Restauramos paquetes sólo para TodoListApp.Api.csproj; esto arrastrará por
#    transitive reference las demás capas (Application, Domain, Infrastructure).
# -------------------------------------------------------------------------------
WORKDIR /src/TodoListApp.Api
RUN dotnet restore TodoListApp.Api.csproj

# -------------------------------------------------------------------------------
# 3) Copiamos TODO el código fuente que queda (Api + Application + Domain +
#    Infrastructure). No se copia ni la carpeta de tests ni el proyecto de consola.
# -------------------------------------------------------------------------------
WORKDIR /src
COPY TodoListApp.Api/            TodoListApp.Api/
COPY TodoListApp.Application/    TodoListApp.Application/
COPY TodoListApp.Domain/         TodoListApp.Domain/
COPY TodoListApp.Infrastructure/ TodoListApp.Infrastructure/

# -------------------------------------------------------------------------------
# 4) Publicamos (publish) la API en modo Release. El resultado va a /app/publish.
# -------------------------------------------------------------------------------
WORKDIR /src/TodoListApp.Api
RUN dotnet publish TodoListApp.Api.csproj -c Release -o /app/publish


################################################################################
# ETAPA 2: RUNTIME con ASP.NET Core Runtime (.NET 8)
################################################################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# -------------------------------------------------------------------------------
# 5) Copiamos desde la etapa "build" lo que quedó en /app/publish
# -------------------------------------------------------------------------------
COPY --from=build /app/publish .

# -------------------------------------------------------------------------------
# 6) Exponemos el puerto 80 y forzamos a Kestrel a escuchar en 0.0.0.0:80
# -------------------------------------------------------------------------------
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# -------------------------------------------------------------------------------
# 7) Punto de entrada: arrancamos la API
# -------------------------------------------------------------------------------
ENTRYPOINT ["dotnet", "TodoListApp.Api.dll"]
