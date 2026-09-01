using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;

namespace ProjetoTarefas.Tests.Fakes;

internal sealed class EtiquetaRepositoryFake : IEtiquetaRepository
{
    public List<Etiqueta> Etiquetas { get; } = [];

    public Task<List<Etiqueta>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Etiquetas.ToList());
    }

    public Task<Etiqueta?> BuscarPorNomeNormalizadoAsync(
        string nomeNormalizado,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Etiquetas.SingleOrDefault(
            etiqueta => etiqueta.NomeNormalizado == nomeNormalizado));
    }

    public Task<List<Etiqueta>> BuscarPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Etiquetas.Where(etiqueta => ids.Contains(etiqueta.Id)).ToList());
    }

    public Task<Etiqueta?> BuscarPorIdAsync(
        int id,
        bool rastrearAlteracoes = false,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Etiquetas.SingleOrDefault(etiqueta => etiqueta.Id == id));
    }

    public void Adicionar(Etiqueta etiqueta) => Etiquetas.Add(etiqueta);

    public void Remover(Etiqueta etiqueta) => Etiquetas.Remove(etiqueta);

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}
