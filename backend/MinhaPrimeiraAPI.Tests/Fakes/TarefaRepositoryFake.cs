using MinhaPrimeiraAPI.Models;
using MinhaPrimeiraAPI.Repositories;

namespace MinhaPrimeiraAPI.Tests.Fakes;

internal sealed class TarefaRepositoryFake : ITarefaRepository
{
    public List<Tarefa> TarefasAtivas { get; } = [];

    public List<Tarefa> TarefasExcluidas { get; } = [];

    public List<HistoricoTarefa> HistoricoTarefas { get; } = [];

    public Tarefa? TarefaRetornadaPorBuscaAtiva { get; set; }

    public Tarefa? TarefaRetornadaPorBuscaIncluindoExcluidas { get; set; }

    public Tarefa? TarefaAdicionada { get; private set; }

    public HistoricoTarefa? HistoricoAdicionado { get; private set; }

    public Tarefa? TarefaRemovida { get; private set; }

    public int QuantidadeChamadasListarAtivas { get; private set; }

    public int QuantidadeChamadasObterResumoAtivas { get; private set; }

    public int QuantidadeChamadasListarExcluidas { get; private set; }

    public int QuantidadeChamadasBuscarAtiva { get; private set; }

    public int QuantidadeChamadasBuscarIncluindoExcluidas { get; private set; }

    public int QuantidadeChamadasListarHistorico { get; private set; }

    public int QuantidadeChamadasAdicionar { get; private set; }

    public int QuantidadeChamadasAdicionarHistorico { get; private set; }

    public int QuantidadeChamadasRemover { get; private set; }

    public int QuantidadeChamadasSalvarAlteracoes { get; private set; }

    public int? UltimoIdBuscadoAtiva { get; private set; }

    public int? UltimoIdBuscadoIncluindoExcluidas { get; private set; }

    public int? UltimoIdHistoricoConsultado { get; private set; }

    public bool UltimaBuscaAtivaRastreouAlteracoes { get; private set; }

    public ConsultaTarefas? UltimaConsultaTarefas { get; private set; }

    public CancellationToken UltimoCancellationToken { get; private set; }

    public ResultadoConsultaTarefas? ResultadoConsultaConfigurado { get; set; }

    public ResultadoResumoTarefas ResultadoResumoConfigurado { get; set; } = new();

    public DateOnly? UltimaDataResumo { get; private set; }

    public Task<ResultadoConsultaTarefas> ListarAtivasAsync(ConsultaTarefas consulta, CancellationToken cancellationToken = default)
    {
        QuantidadeChamadasListarAtivas++;
        UltimaConsultaTarefas = consulta;
        UltimoCancellationToken = cancellationToken;

        return Task.FromResult(ResultadoConsultaConfigurado ?? new ResultadoConsultaTarefas
        {
            Itens = TarefasAtivas,
            TotalItens = TarefasAtivas.Count
        });
    }

    public Task<ResultadoResumoTarefas> ObterResumoAtivasAsync(DateOnly hoje, CancellationToken cancellationToken = default)
    {
        QuantidadeChamadasObterResumoAtivas++;
        UltimaDataResumo = hoje;
        UltimoCancellationToken = cancellationToken;

        return Task.FromResult(ResultadoResumoConfigurado);
    }

    public Task<List<Tarefa>> ListarExcluidasAsync(CancellationToken cancellationToken = default)
    {
        QuantidadeChamadasListarExcluidas++;
        UltimoCancellationToken = cancellationToken;

        return Task.FromResult(TarefasExcluidas);
    }

    public Task<Tarefa?> BuscarAtivaPorIdAsync(
        int id,
        bool rastrearAlteracoes = false,
        CancellationToken cancellationToken = default)
    {
        QuantidadeChamadasBuscarAtiva++;
        UltimoIdBuscadoAtiva = id;
        UltimaBuscaAtivaRastreouAlteracoes = rastrearAlteracoes;
        UltimoCancellationToken = cancellationToken;

        return Task.FromResult(TarefaRetornadaPorBuscaAtiva);
    }

    public Task<Tarefa?> BuscarIncluindoExcluidasPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        QuantidadeChamadasBuscarIncluindoExcluidas++;
        UltimoIdBuscadoIncluindoExcluidas = id;
        UltimoCancellationToken = cancellationToken;

        return Task.FromResult(
            TarefaRetornadaPorBuscaIncluindoExcluidas
        );
    }

    public Task<List<HistoricoTarefa>> ListarHistoricoAsync(int tarefaId, CancellationToken cancellationToken = default)
    {
        QuantidadeChamadasListarHistorico++;
        UltimoIdHistoricoConsultado = tarefaId;
        UltimoCancellationToken = cancellationToken;

        return Task.FromResult(HistoricoTarefas);
    }

    public void Adicionar(Tarefa tarefa)
    {
        QuantidadeChamadasAdicionar++;
        TarefaAdicionada = tarefa;
    }

    public void AdicionarHistorico(HistoricoTarefa historico)
    {
        QuantidadeChamadasAdicionarHistorico++;
        HistoricoAdicionado = historico;
        HistoricoTarefas.Add(historico);
    }

    public void Remover(Tarefa tarefa)
    {
        QuantidadeChamadasRemover++;
        TarefaRemovida = tarefa;
    }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        QuantidadeChamadasSalvarAlteracoes++;
        UltimoCancellationToken = cancellationToken;

        return Task.FromResult(1);
    }
}
