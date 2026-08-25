using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.DTOs.Responses;

namespace MinhaPrimeiraAPI.Services;

public interface IProjetoService
{
    Task<List<ProjetoResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<ProjetoResponse> CriarAsync(CriarProjetoRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
