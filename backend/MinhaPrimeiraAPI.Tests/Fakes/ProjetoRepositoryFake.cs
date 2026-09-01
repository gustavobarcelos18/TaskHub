using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;

namespace ProjetoTarefas.Tests.Fakes;

internal sealed class ProjetoRepositoryFake : IProjetoRepository
{
    public List<Projeto> Projetos { get; } = [];

    public Task<List<Projeto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Projetos.ToList());
    }

    public Task<Projeto?> BuscarPorNomeNormalizadoAsync(
        string nomeNormalizado,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Projetos.SingleOrDefault(
            projeto => projeto.NomeNormalizado == nomeNormalizado));
    }

    public Task<Projeto?> BuscarPorIdAsync(
        int id,
        bool rastrearAlteracoes = false,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Projetos.SingleOrDefault(projeto => projeto.Id == id));
    }

    public void Adicionar(Projeto projeto) => Projetos.Add(projeto);

    public void Remover(Projeto projeto) => Projetos.Remove(projeto);

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}
