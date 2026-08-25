using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraAPI.Data;
using MinhaPrimeiraAPI.Models;

namespace MinhaPrimeiraAPI.Repositories;

public class ProjetoRepository(AppDbContext context) : IProjetoRepository
{
    public Task<List<Projeto>> ListarAsync(CancellationToken cancellationToken = default) => context.Projetos.AsNoTracking().OrderBy(projeto => projeto.Nome).ThenBy(projeto => projeto.Id).ToListAsync(cancellationToken);
    public Task<Projeto?> BuscarPorNomeNormalizadoAsync(string nomeNormalizado, CancellationToken cancellationToken = default) => context.Projetos.AsNoTracking().SingleOrDefaultAsync(projeto => projeto.NomeNormalizado == nomeNormalizado, cancellationToken);
    public async Task<Projeto?> BuscarPorIdAsync(int id, bool rastrearAlteracoes = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Projeto> consulta = context.Projetos;
        if (!rastrearAlteracoes) consulta = consulta.AsNoTracking();
        return await consulta.SingleOrDefaultAsync(projeto => projeto.Id == id, cancellationToken);
    }
    public void Adicionar(Projeto projeto) => context.Projetos.Add(projeto);
    public void Remover(Projeto projeto) => context.Projetos.Remove(projeto);
    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}
