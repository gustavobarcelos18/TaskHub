using Microsoft.Data.Sqlite;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;

namespace ProjetoTarefas.Tests.Repositories;

public sealed class LogRepositoryTests
{
    [Fact]
    public async Task ConsultarAsync_DeveFiltrarOrdenarEPaginarSemIncluirConsultasDoDashboard()
    {
        var caminho = Path.Combine(Path.GetTempPath(), $"taskhub-logs-{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = new SqliteConnectionStringBuilder { DataSource = caminho, Pooling = false }.ToString();
            await using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE LogEventos (Id INTEGER PRIMARY KEY, TimestampUtc TEXT, Level TEXT, EventName TEXT, Message TEXT, UserId TEXT, UserName TEXT, Method TEXT, Path TEXT, StatusCode INTEGER, ElapsedMs REAL, TraceId TEXT, SafePropertiesJson TEXT); INSERT INTO LogEventos VALUES (1, '2026-08-27T10:00:00.0000000Z', 'Information', 'A', 'primeiro', 'u1', 'ana', 'GET', '/api/tarefas', 200, 4, 'trace-1', NULL); INSERT INTO LogEventos VALUES (2, '2026-08-27T11:00:00.0000000Z', 'Warning', 'B', 'dashboard', 'u1', 'ana', 'GET', '/api/logs', 200, 2, 'trace-2', NULL); INSERT INTO LogEventos VALUES (3, '2026-08-27T12:00:00.0000000Z', 'Information', 'C', 'segundo', 'u2', 'bia', 'POST', '/api/tarefas', 201, 8, 'trace-3', NULL);";
                await command.ExecuteNonQueryAsync();
            }

            var resultado = await new LogRepository(connectionString).ConsultarAsync(new ConsultaLogsRequest { Page = 1, PageSize = 1, Level = "Information" });

            Assert.Equal(2, resultado.Total);
            var item = Assert.Single(resultado.Itens);
            Assert.Equal(3, item.Id);
            Assert.Equal("bia", item.UserName);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(caminho)) File.Delete(caminho);
        }
    }
}
