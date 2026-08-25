using MinhaPrimeiraAPI.Models;
using MinhaPrimeiraAPI.Repositories;

namespace MinhaPrimeiraAPI.Tests.Fakes;

internal sealed class TarefaRepositoryFake : ITarefaRepository
{
    public List<Tarefa> TarefasAtivas { get; } = [];

    public Tarefa? TarefaRetornadaPorBuscaAtiva { get; set; }

    public Tarefa? TarefaRetornadaPorBuscaIncluindoExcluidas { get; set; }

    public Tarefa? TarefaAdicionada { get; private set; }

    public Tarefa? TarefaRemovida { get; private set; }

    public int QuantidadeChamadasListarAtivas { get; private set; }

    public int QuantidadeChamadasBuscarAtiva { get; private set; }

    public int QuantidadeChamadasBuscarIncluindoExcluidas { get; private set; }

    public int QuantidadeChamadasAdicionar { get; private set; }

    public int QuantidadeChamadasRemover { get; private set; }

    public int QuantidadeChamadasSalvarAlteracoes { get; private set; }

    public int? UltimoIdBuscadoAtiva { get; private set; }

    public int? UltimoIdBuscadoIncluindoExcluidas { get; private set; }

    public bool UltimaBuscaAtivaRastreouAlteracoes { get; private set; }

    public Task<List<Tarefa>> ListarAtivasAsync()
    {
        QuantidadeChamadasListarAtivas++;

        return Task.FromResult(TarefasAtivas);
    }

    public Task<Tarefa?> BuscarAtivaPorIdAsync(
        int id,
        bool rastrearAlteracoes = false)
    {
        QuantidadeChamadasBuscarAtiva++;
        UltimoIdBuscadoAtiva = id;
        UltimaBuscaAtivaRastreouAlteracoes = rastrearAlteracoes;

        return Task.FromResult(TarefaRetornadaPorBuscaAtiva);
    }

    public Task<Tarefa?> BuscarIncluindoExcluidasPorIdAsync(int id)
    {
        QuantidadeChamadasBuscarIncluindoExcluidas++;
        UltimoIdBuscadoIncluindoExcluidas = id;

        return Task.FromResult(
            TarefaRetornadaPorBuscaIncluindoExcluidas
        );
    }

    public void Adicionar(Tarefa tarefa)
    {
        QuantidadeChamadasAdicionar++;
        TarefaAdicionada = tarefa;
    }

    public void Remover(Tarefa tarefa)
    {
        QuantidadeChamadasRemover++;
        TarefaRemovida = tarefa;
    }

    public Task<int> SalvarAlteracoesAsync()
    {
        QuantidadeChamadasSalvarAlteracoes++;

        return Task.FromResult(1);
    }
}