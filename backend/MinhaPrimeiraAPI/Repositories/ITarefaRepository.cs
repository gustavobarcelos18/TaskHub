using ProjetoTarefas.Models;

namespace ProjetoTarefas.Repositories;

public interface ITarefaRepository
{
    Task<ResultadoConsultaTarefas> ListarAtivasAsync(ConsultaTarefas consulta, CancellationToken cancellationToken = default);

    Task<ResultadoResumoTarefas> ObterResumoAtivasAsync(DateOnly hoje, CancellationToken cancellationToken = default);

    Task<List<Tarefa>> ListarExcluidasAsync(CancellationToken cancellationToken = default);

    Task<Tarefa?> BuscarAtivaPorIdAsync(
        int id,
        bool rastrearAlteracoes = false,
        CancellationToken cancellationToken = default
    );

    Task<Tarefa?> BuscarIncluindoExcluidasPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<HistoricoTarefa>> ListarHistoricoAsync(int tarefaId, CancellationToken cancellationToken = default);

    void Adicionar(Tarefa tarefa);

    void AdicionarHistorico(HistoricoTarefa historico);

    void Remover(Tarefa tarefa);

    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
