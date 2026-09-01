using Microsoft.Data.Sqlite;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.Models;

namespace ProjetoTarefas.Repositories;

public sealed class LogRepository(string connectionString) : ILogRepository
{
    public async Task<(List<LogEvento> Itens, int Total)> ConsultarAsync(ConsultaLogsRequest consulta, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var countCommand = CriarCommand(connection, consulta, countOnly: true);
        var total = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

        await using var command = CriarCommand(connection, consulta, countOnly: false);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var itens = new List<LogEvento>();
        while (await reader.ReadAsync(cancellationToken))
        {
            itens.Add(new LogEvento
            {
                Id = reader.GetInt64(0), TimestampUtc = DateTime.Parse(reader.GetString(1)), Level = reader.GetString(2),
                EventName = reader.GetString(3), Message = reader.GetString(4), UserId = Texto(reader, 5), UserName = Texto(reader, 6),
                Method = Texto(reader, 7), Path = Texto(reader, 8), StatusCode = Inteiro(reader, 9), ElapsedMs = Decimal(reader, 10),
                TraceId = Texto(reader, 11), SafePropertiesJson = Texto(reader, 12)
            });
        }
        return (itens, total);
    }

    private static SqliteCommand CriarCommand(SqliteConnection connection, ConsultaLogsRequest consulta, bool countOnly)
    {
        var command = connection.CreateCommand();
        var where = new List<string> { "Path <> '/api/logs'" };
        Adicionar(command, where, "Level = $level", "$level", consulta.Level);
        Adicionar(command, where, "(UserId LIKE $user OR UserName LIKE $user)", "$user", consulta.User is null ? null : $"%{consulta.User.Trim()}%");
        Adicionar(command, where, "TimestampUtc >= $start", "$start", consulta.StartDate?.ToUniversalTime().ToString("O"));
        Adicionar(command, where, "TimestampUtc < $end", "$end", consulta.EndDate?.ToUniversalTime().AddDays(1).ToString("O"));
        Adicionar(command, where, "StatusCode = $status", "$status", consulta.StatusCode);
        Adicionar(command, where, "Method = $method", "$method", consulta.Method?.Trim().ToUpperInvariant());
        Adicionar(command, where, "Path LIKE $path", "$path", consulta.Path is null ? null : $"%{consulta.Path.Trim()}%");
        Adicionar(command, where, "TraceId = $trace", "$trace", consulta.TraceId?.Trim());
        Adicionar(command, where, "(Message LIKE $text OR EventName LIKE $text)", "$text", consulta.Text is null ? null : $"%{consulta.Text.Trim()}%");
        command.CommandText = countOnly
            ? $"SELECT COUNT(*) FROM LogEventos WHERE {string.Join(" AND ", where)}"
            : $"SELECT Id, TimestampUtc, Level, EventName, Message, UserId, UserName, Method, Path, StatusCode, ElapsedMs, TraceId, SafePropertiesJson FROM LogEventos WHERE {string.Join(" AND ", where)} ORDER BY TimestampUtc DESC, Id DESC LIMIT $limit OFFSET $offset";
        if (!countOnly)
        {
            command.Parameters.AddWithValue("$limit", consulta.PageSize);
            command.Parameters.AddWithValue("$offset", (consulta.Page - 1) * consulta.PageSize);
        }
        return command;
    }

    private static void Adicionar(SqliteCommand command, List<string> where, string condition, string name, object? value)
    {
        if (value is null || value is string text && string.IsNullOrWhiteSpace(text)) return;
        where.Add(condition);
        command.Parameters.AddWithValue(name, value);
    }

    private static string? Texto(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetString(index);
    private static int? Inteiro(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetInt32(index);
    private static double? Decimal(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetDouble(index);
}
