using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;

namespace ProjetoTarefas.Services;

public interface IProjetoService
{
    Task<List<ProjetoResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<ProjetoResponse> CriarAsync(CriarProjetoRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
