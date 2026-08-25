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

    public async Task<ResultadoConsultaTarefas> ListarAtivasAsync(
        ConsultaTarefas consulta,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tarefa> tarefas = _context.Tarefas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(consulta.Busca))
        {
            tarefas = tarefas.Where(tarefa =>
                tarefa.Descricao.Contains(consulta.Busca));
        }

        if (!string.IsNullOrWhiteSpace(consulta.Situacao))
        {
            tarefas = tarefas.Where(tarefa =>
                tarefa.Situacao == consulta.Situacao);
        }

        if (!string.IsNullOrWhiteSpace(consulta.Prioridade))
        {
            tarefas = tarefas.Where(tarefa => tarefa.Prioridade == consulta.Prioridade);
        }

        if (consulta.EtiquetaId is not null)
        {
            tarefas = tarefas.Where(tarefa => tarefa.Etiquetas.Any(etiqueta => etiqueta.Id == consulta.EtiquetaId));
        }

        tarefas = consulta.Prazo switch
        {
            FiltroPrazoTarefa.Vencidas => tarefas.Where(tarefa =>
                tarefa.DataVencimento < consulta.Hoje && tarefa.Situacao != SituacoesTarefa.Concluida),
            FiltroPrazoTarefa.VencemHoje => tarefas.Where(tarefa =>
                tarefa.DataVencimento == consulta.Hoje && tarefa.Situacao != SituacoesTarefa.Concluida),
            FiltroPrazoTarefa.Proximas => tarefas.Where(tarefa =>
                tarefa.DataVencimento > consulta.Hoje && tarefa.Situacao != SituacoesTarefa.Concluida),
            FiltroPrazoTarefa.SemVencimento => tarefas.Where(tarefa => tarefa.DataVencimento == null),
            _ => tarefas
        };

        var totalItens = await tarefas.CountAsync(cancellationToken);
        tarefas = Ordenar(tarefas, consulta);

        var itens = await tarefas
            .Skip((consulta.Pagina - 1) * consulta.TamanhoPagina)
            .Take(consulta.TamanhoPagina)
            .Include(tarefa => tarefa.Etiquetas)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return new ResultadoConsultaTarefas
        {
            Itens = itens,
            TotalItens = totalItens
        };
    }

    public async Task<ResultadoResumoTarefas> ObterResumoAtivasAsync(DateOnly hoje, CancellationToken cancellationToken = default)
    {
        var resumo = await _context.Tarefas
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(grupo => new ResultadoResumoTarefas
            {
                Total = grupo.Count(),
                Pendentes = grupo.Count(tarefa => tarefa.Situacao == SituacoesTarefa.Pendente),
                EmAndamento = grupo.Count(tarefa => tarefa.Situacao == SituacoesTarefa.EmAndamento),
                Concluidas = grupo.Count(tarefa => tarefa.Situacao == SituacoesTarefa.Concluida),
                Vencidas = grupo.Count(tarefa => tarefa.DataVencimento < hoje && tarefa.Situacao != SituacoesTarefa.Concluida),
                VencemHoje = grupo.Count(tarefa => tarefa.DataVencimento == hoje && tarefa.Situacao != SituacoesTarefa.Concluida),
                Proximas = grupo.Count(tarefa => tarefa.DataVencimento > hoje && tarefa.Situacao != SituacoesTarefa.Concluida)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return resumo ?? new ResultadoResumoTarefas();
    }

    private static IQueryable<Tarefa> Ordenar(
        IQueryable<Tarefa> tarefas,
        ConsultaTarefas consulta)
    {
        return (consulta.OrdenarPor, consulta.Direcao) switch
        {
            (CampoOrdenacaoTarefa.Descricao, DirecaoOrdenacao.Asc) => tarefas.OrderBy(tarefa => tarefa.Descricao).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Descricao, DirecaoOrdenacao.Desc) => tarefas.OrderByDescending(tarefa => tarefa.Descricao).ThenByDescending(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Situacao, DirecaoOrdenacao.Asc) => tarefas.OrderBy(tarefa => tarefa.Situacao).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Situacao, DirecaoOrdenacao.Desc) => tarefas.OrderByDescending(tarefa => tarefa.Situacao).ThenByDescending(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Prioridade, DirecaoOrdenacao.Asc) => tarefas.OrderBy(tarefa =>
                tarefa.Prioridade == PrioridadesTarefa.Baixa ? 0 : tarefa.Prioridade == PrioridadesTarefa.Media ? 1 : 2).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Prioridade, DirecaoOrdenacao.Desc) => tarefas.OrderByDescending(tarefa =>
                tarefa.Prioridade == PrioridadesTarefa.Baixa ? 0 : tarefa.Prioridade == PrioridadesTarefa.Media ? 1 : 2).ThenByDescending(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.DataVencimento, DirecaoOrdenacao.Asc) => tarefas.OrderBy(tarefa => tarefa.DataVencimento == null).ThenBy(tarefa => tarefa.DataVencimento).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.DataVencimento, DirecaoOrdenacao.Desc) => tarefas.OrderBy(tarefa => tarefa.DataVencimento == null).ThenByDescending(tarefa => tarefa.DataVencimento).ThenByDescending(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.UltimaAtualizacao, DirecaoOrdenacao.Asc) => tarefas.OrderBy(tarefa => tarefa.ModificadaEm ?? tarefa.CriadaEm).ThenBy(tarefa => tarefa.Id),
            _ => tarefas.OrderByDescending(tarefa => tarefa.ModificadaEm ?? tarefa.CriadaEm).ThenByDescending(tarefa => tarefa.Id)
        };
    }

    public async Task<List<Tarefa>> ListarExcluidasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tarefas
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(tarefa => tarefa.ExcluidaEm != null)
            .Include(tarefa => tarefa.Etiquetas)
            .AsSplitQuery()
            .OrderByDescending(tarefa => tarefa.ExcluidaEm)
            .ThenByDescending(tarefa => tarefa.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tarefa?> BuscarAtivaPorIdAsync(
        int id,
        bool rastrearAlteracoes = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tarefa> consulta = _context.Tarefas;

        if (!rastrearAlteracoes)
        {
            consulta = consulta.AsNoTracking();
        }

        return await consulta
            .Include(tarefa => tarefa.Etiquetas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
            tarefa => tarefa.Id == id,
            cancellationToken
        );
    }

    public async Task<Tarefa?> BuscarIncluindoExcluidasPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tarefas
            .IgnoreQueryFilters()
            .Include(tarefa => tarefa.Etiquetas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                tarefa => tarefa.Id == id,
                cancellationToken
        );
    }

    public async Task<List<HistoricoTarefa>> ListarHistoricoAsync(int tarefaId, CancellationToken cancellationToken = default)
    {
        return await _context.HistoricosTarefas
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(historico => historico.TarefaId == tarefaId)
            .OrderByDescending(historico => historico.CriadoEm)
            .ThenByDescending(historico => historico.Id)
            .ToListAsync(cancellationToken);
    }

    public void Adicionar(Tarefa tarefa)
    {
        _context.Tarefas.Add(tarefa);
    }

    public void AdicionarHistorico(HistoricoTarefa historico)
    {
        _context.HistoricosTarefas.Add(historico);
    }

    public void Remover(Tarefa tarefa)
    {
        _context.Tarefas.Remove(tarefa);
    }

    public async Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
