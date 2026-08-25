using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.DTOs.Responses;

namespace MinhaPrimeiraAPI.Services;

public interface ITarefaService
{
    Task<TarefasPaginadasResponse> ListarAsync(ConsultaTarefasRequest consulta, CancellationToken cancellationToken = default);

    Task<ResumoTarefasResponse> ObterResumoAsync(CancellationToken cancellationToken = default);

    Task<List<TarefaResponse>> ListarExcluidasAsync(CancellationToken cancellationToken = default);

    Task<TarefaResponse?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<HistoricoTarefaResponse>?> ListarHistoricoAsync(int id, CancellationToken cancellationToken = default);

    Task<TarefaResponse> CriarAsync(CriarTarefaRequest novaTarefa, CancellationToken cancellationToken = default);

    Task<TarefaResponse?> AtualizarAsync(
        int id,
        AtualizarTarefaRequest dadosAtualizados,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExcluirLogicamenteAsync(int id, CancellationToken cancellationToken = default);

    Task<ResultadoRestauracao> RestaurarAsync(int id, CancellationToken cancellationToken = default);

    Task<ResultadoExclusaoPermanente> ExcluirPermanentementeAsync(int id, CancellationToken cancellationToken = default);
}
