using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.Models;

namespace ProjetoTarefas.Repositories;

public interface ILogRepository
{
    Task<(List<LogEvento> Itens, int Total)> ConsultarAsync(ConsultaLogsRequest consulta, CancellationToken cancellationToken = default);
}
