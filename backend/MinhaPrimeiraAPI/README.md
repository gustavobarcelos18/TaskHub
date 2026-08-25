# MinhaPrimeiraAPI

Web API didática de gerenciamento de tarefas usando .NET 10, ASP.NET Core, Entity Framework Core, SQLite, Serilog e Swagger.

## Fluxo

```text
Cliente HTTP
    ↓
TarefasController
    ↓ ITarefaService
TarefaService
    ↓ ITarefaRepository
TarefaRepository
    ↓
AppDbContext
    ↓
SQLite
```

As interfaces definem os contratos. As classes `TarefaService` e `TarefaRepository` contêm as implementações reais.

## Executar

```powershell
dotnet restore
dotnet build
dotnet run
```

Swagger em ambiente de desenvolvimento:

```text
http://localhost:5025/swagger
```

## Endpoints

- `GET /api/tarefas`
- `GET /api/tarefas/{id}`
- `POST /api/tarefas`
- `PUT /api/tarefas/{id}`
- `DELETE /api/tarefas/{id}`
- `DELETE /api/tarefas/{id}/permanente`
