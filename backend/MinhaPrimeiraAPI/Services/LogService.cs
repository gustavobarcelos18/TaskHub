using System.Text.Json;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Repositories;

namespace ProjetoTarefas.Services;

public sealed class LogService(ILogRepository repository)
{
    private static readonly HashSet<string> Niveis = ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"];

    public async Task<LogsPaginadosResponse> ConsultarAsync(ConsultaLogsRequest consulta, CancellationToken cancellationToken = default)
    {
        if (consulta.Page < 1 || consulta.PageSize is < 1 or > 100) throw new ArgumentException("Page deve ser maior que zero e PageSize deve estar entre 1 e 100.");
        if (consulta.Level is not null && !Niveis.Contains(consulta.Level, StringComparer.OrdinalIgnoreCase)) throw new ArgumentException("Level inválido.");
        var (itens, total) = await repository.ConsultarAsync(consulta, cancellationToken);
        return new LogsPaginadosResponse
        {
            Itens = itens.Select(Mapear).ToList(), PaginaAtual = consulta.Page, TamanhoPagina = consulta.PageSize,
            TotalItens = total, TotalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)consulta.PageSize)
        };
    }

    private static LogEventoResponse Mapear(Models.LogEvento item) => new()
    {
        Id = item.Id, Timestamp = DateTime.SpecifyKind(item.TimestampUtc, DateTimeKind.Utc), Level = item.Level,
        EventName = item.EventName, Message = item.Message, UserId = item.UserId, UserName = item.UserName,
        Method = item.Method, Path = item.Path, StatusCode = item.StatusCode, ElapsedMs = item.ElapsedMs, TraceId = item.TraceId,
        Properties = string.IsNullOrWhiteSpace(item.SafePropertiesJson)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, string>>(item.SafePropertiesJson) ?? []
    };
}
