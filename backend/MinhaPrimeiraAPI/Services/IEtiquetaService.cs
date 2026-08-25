using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.DTOs.Responses;

namespace MinhaPrimeiraAPI.Services;

public interface IEtiquetaService
{
    Task<List<EtiquetaResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<EtiquetaResponse> CriarAsync(CriarEtiquetaRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
