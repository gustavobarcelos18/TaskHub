using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;

namespace ProjetoTarefas.Services;

public interface IEtiquetaService
{
    Task<List<EtiquetaResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<EtiquetaResponse> CriarAsync(CriarEtiquetaRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
