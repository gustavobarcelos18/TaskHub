using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraAPI.Data;
using MinhaPrimeiraAPI.Models;

namespace MinhaPrimeiraAPI.Repositories;

public class TarefaRepository : ITarefaRepository
{
    private readonly AppDbContext _context;

    public TarefaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tarefa>> ListarAtivasAsync()
    {
        return await _context.Tarefas
            .AsNoTracking()
            .OrderBy(tarefa => tarefa.Id)
            .ToListAsync();
    }

    public async Task<Tarefa?> BuscarAtivaPorIdAsync(
        int id,
        bool rastrearAlteracoes = false)
    {
        IQueryable<Tarefa> consulta = _context.Tarefas;

        if (!rastrearAlteracoes)
        {
            consulta = consulta.AsNoTracking();
        }

        return await consulta.FirstOrDefaultAsync(
            tarefa => tarefa.Id == id
        );
    }

    public async Task<Tarefa?> BuscarIncluindoExcluidasPorIdAsync(int id)
    {
        return await _context.Tarefas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                tarefa => tarefa.Id == id
            );
    }

    public void Adicionar(Tarefa tarefa)
    {
        _context.Tarefas.Add(tarefa);
    }

    public void Remover(Tarefa tarefa)
    {
        _context.Tarefas.Remove(tarefa);
    }

    public async Task<int> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
