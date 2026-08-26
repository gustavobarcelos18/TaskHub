using Microsoft.EntityFrameworkCore;
using ProjetoTarefas.Data;
using ProjetoTarefas.Models;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Repositories;

public class EtiquetaRepository(AppDbContext context, IUsuarioAtual? usuarioAtual = null) : IEtiquetaRepository
{
    public Task<List<Etiqueta>> ListarAsync(CancellationToken cancellationToken = default) => context.Etiquetas.AsNoTracking().Where(etiqueta => usuarioAtual == null || etiqueta.UsuarioId == usuarioAtual.Id).OrderBy(etiqueta => etiqueta.Nome).ThenBy(etiqueta => etiqueta.Id).ToListAsync(cancellationToken);
    public Task<Etiqueta?> BuscarPorNomeNormalizadoAsync(string nomeNormalizado, CancellationToken cancellationToken = default) => context.Etiquetas.AsNoTracking().SingleOrDefaultAsync(etiqueta => (usuarioAtual == null || etiqueta.UsuarioId == usuarioAtual.Id) && etiqueta.NomeNormalizado == nomeNormalizado, cancellationToken);
    public Task<List<Etiqueta>> BuscarPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => context.Etiquetas.Where(etiqueta => (usuarioAtual == null || etiqueta.UsuarioId == usuarioAtual.Id) && ids.Contains(etiqueta.Id)).ToListAsync(cancellationToken);
    public async Task<Etiqueta?> BuscarPorIdAsync(int id, bool rastrearAlteracoes = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Etiqueta> consulta = context.Etiquetas;
        if (!rastrearAlteracoes) consulta = consulta.AsNoTracking();
        return await consulta.SingleOrDefaultAsync(etiqueta => etiqueta.Id == id && (usuarioAtual == null || etiqueta.UsuarioId == usuarioAtual.Id), cancellationToken);
    }
    public void Adicionar(Etiqueta etiqueta) => context.Etiquetas.Add(etiqueta);
    public void Remover(Etiqueta etiqueta) => context.Etiquetas.Remove(etiqueta);
    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}
