using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.DTOs.Responses;

namespace MinhaPrimeiraAPI.Services;

public interface ITarefaService
{
    Task<List<TarefaResponse>> ListarAsync();

    Task<TarefaResponse?> BuscarPorIdAsync(int id);

    Task<TarefaResponse> CriarAsync(CriarTarefaRequest novaTarefa);

    Task<TarefaResponse?> AtualizarAsync(
        int id,
        AtualizarTarefaRequest dadosAtualizados
    );

    Task<bool> ExcluirLogicamenteAsync(int id);

    Task<ResultadoExclusaoPermanente> ExcluirPermanentementeAsync(int id);
}
