using Microsoft.EntityFrameworkCore;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;
using ProjetoTarefas.Tests.Infrastructure;

namespace ProjetoTarefas.Tests.Repositories;

public sealed class TarefaRepositoryTests
{
    private static readonly DateOnly Hoje = new(2030, 1, 2);
    private static readonly DateTime CriadaEm = new(2030, 1, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ListarAtivasAsync_DeveAplicarFiltroGlobalEExcluirTarefaExcluidaDoResumo()
    {
        await using var database = await CriarBancoAsync();
        var ativa = NovaTarefa("Ativa", SituacoesTarefa.Pendente);
        var excluida = NovaTarefa("Excluída", SituacoesTarefa.Concluida, excluidaEm: CriadaEm.AddHours(1));

        await SalvarTarefasAsync(database, ativa, excluida);

        await using var context = database.CreateContext();
        var repository = new TarefaRepository(context);

        var resultado = await repository.ListarAtivasAsync(Consulta());
        var resumo = await repository.ObterResumoAtivasAsync(Hoje);

        Assert.Equal([ativa.Id], resultado.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal(1, resumo.Total);
        Assert.Equal(1, resumo.Pendentes);

        await using var verificacao = database.CreateContext();
        Assert.Equal(2, await verificacao.Tarefas.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task MetodosComIgnoreQueryFilters_DevemEncontrarTarefaExcluidaESeuHistorico()
    {
        await using var database = await CriarBancoAsync();
        var excluida = NovaTarefa("Na lixeira", SituacoesTarefa.Pendente, excluidaEm: CriadaEm.AddHours(2));
        var historico = new HistoricoTarefa { Tarefa = excluida, Tipo = TiposHistoricoTarefa.Exclusao, CriadoEm = CriadaEm.AddHours(2) };

        await using (var context = database.CreateContext())
        {
            context.Add(historico);
            await context.SaveChangesAsync();
        }

        await using var consultaContext = database.CreateContext();
        var repository = new TarefaRepository(consultaContext);

        var lixeira = await repository.ListarExcluidasAsync();
        var encontrada = await repository.BuscarIncluindoExcluidasPorIdAsync(excluida.Id);
        var historicos = await repository.ListarHistoricoAsync(excluida.Id);

        Assert.Equal(excluida.Id, Assert.Single(lixeira).Id);
        Assert.Equal(excluida.Id, encontrada?.Id);
        Assert.Equal(historico.Id, Assert.Single(historicos).Id);
    }

    [Fact]
    public async Task ListarAtivasAsync_DeveCombinarBuscaSituacaoPrioridadeEPrazoNoSQLite()
    {
        await using var database = await CriarBancoAsync();
        var esperada = NovaTarefa("Relatório atrasado", SituacoesTarefa.EmAndamento, PrioridadesTarefa.Alta, Hoje.AddDays(-1));
        var prioridadeDiferente = NovaTarefa("Relatório baixa", SituacoesTarefa.EmAndamento, PrioridadesTarefa.Baixa, Hoje.AddDays(-1));
        var situacaoDiferente = NovaTarefa("Relatório concluído", SituacoesTarefa.Concluida, PrioridadesTarefa.Alta, Hoje.AddDays(-1));
        var buscaDiferente = NovaTarefa("Planejamento atrasado", SituacoesTarefa.EmAndamento, PrioridadesTarefa.Alta, Hoje.AddDays(-1));

        await SalvarTarefasAsync(database, esperada, prioridadeDiferente, situacaoDiferente, buscaDiferente);

        await using var context = database.CreateContext();
        var resultado = await new TarefaRepository(context).ListarAtivasAsync(Consulta(
            busca: "Relatório",
            situacao: SituacoesTarefa.EmAndamento,
            prioridade: PrioridadesTarefa.Alta,
            prazo: FiltroPrazoTarefa.Vencidas));

        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal(esperada.Id, Assert.Single(resultado.Itens).Id);
    }

    [Fact]
    public async Task ListarAtivasAsync_DeveRetornarBuscaEncontradaENenhumItemParaTermoInexistente()
    {
        await using var database = await CriarBancoAsync();
        var encontrada = NovaTarefa("Preparar relatório", SituacoesTarefa.Pendente);
        var outra = NovaTarefa("Organizar agenda", SituacoesTarefa.Pendente);
        await SalvarTarefasAsync(database, encontrada, outra);

        await using var context = database.CreateContext();
        var repository = new TarefaRepository(context);

        var resultadoEncontrado = await repository.ListarAtivasAsync(Consulta(busca: "relatório"));
        var resultadoInexistente = await repository.ListarAtivasAsync(Consulta(busca: "inexistente"));

        Assert.Equal(encontrada.Id, Assert.Single(resultadoEncontrado.Itens).Id);
        Assert.Equal(0, resultadoInexistente.TotalItens);
        Assert.Empty(resultadoInexistente.Itens);
    }

    [Fact]
    public async Task ListarAtivasAsync_DeveFiltrarPrazosECompararDateOnlyIncluindoNulos()
    {
        await using var database = await CriarBancoAsync();
        var vencida = NovaTarefa("Vencida", SituacoesTarefa.Pendente, dataVencimento: Hoje.AddDays(-1));
        var hoje = NovaTarefa("Hoje", SituacoesTarefa.Pendente, dataVencimento: Hoje);
        var proxima = NovaTarefa("Próxima", SituacoesTarefa.Pendente, dataVencimento: Hoje.AddDays(1));
        var semVencimento = NovaTarefa("Sem vencimento", SituacoesTarefa.Pendente);
        var vencidaConcluida = NovaTarefa("Vencida concluída", SituacoesTarefa.Concluida, dataVencimento: Hoje.AddDays(-1));

        await SalvarTarefasAsync(database, vencida, hoje, proxima, semVencimento, vencidaConcluida);

        await using var context = database.CreateContext();
        var repository = new TarefaRepository(context);

        var vencidas = await repository.ListarAtivasAsync(Consulta(prazo: FiltroPrazoTarefa.Vencidas));
        var vencemHoje = await repository.ListarAtivasAsync(Consulta(prazo: FiltroPrazoTarefa.VencemHoje));
        var proximas = await repository.ListarAtivasAsync(Consulta(prazo: FiltroPrazoTarefa.Proximas));
        var semPrazo = await repository.ListarAtivasAsync(Consulta(prazo: FiltroPrazoTarefa.SemVencimento));
        var resumo = await repository.ObterResumoAtivasAsync(Hoje);

        Assert.Equal([vencida.Id], vencidas.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal([hoje.Id], vencemHoje.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal([proxima.Id], proximas.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal([semVencimento.Id], semPrazo.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal(1, resumo.Vencidas);
        Assert.Equal(1, resumo.VencemHoje);
        Assert.Equal(1, resumo.Proximas);
    }

    [Fact]
    public async Task DataVencimento_DevePersistirSerLidaEOrdenadaComNuloAoFinal()
    {
        await using var database = await CriarBancoAsync();
        var futura = NovaTarefa("Futura", SituacoesTarefa.Pendente, dataVencimento: Hoje.AddDays(2));
        var passada = NovaTarefa("Passada", SituacoesTarefa.Pendente, dataVencimento: Hoje.AddDays(-2));
        var semPrazo = NovaTarefa("Sem prazo", SituacoesTarefa.Pendente);

        await SalvarTarefasAsync(database, futura, passada, semPrazo);

        await using var contextoLeitura = database.CreateContext();
        var repository = new TarefaRepository(contextoLeitura);
        var crescente = await repository.ListarAtivasAsync(Consulta(ordenarPor: CampoOrdenacaoTarefa.DataVencimento, direcao: DirecaoOrdenacao.Asc));
        var decrescente = await repository.ListarAtivasAsync(Consulta(ordenarPor: CampoOrdenacaoTarefa.DataVencimento, direcao: DirecaoOrdenacao.Desc));

        Assert.Equal(Hoje.AddDays(-2), (await contextoLeitura.Tarefas.SingleAsync(tarefa => tarefa.Id == passada.Id)).DataVencimento);
        Assert.Equal([passada.Id, futura.Id, semPrazo.Id], crescente.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal([futura.Id, passada.Id, semPrazo.Id], decrescente.Itens.Select(tarefa => tarefa.Id));
    }

    [Fact]
    public async Task ListarAtivasAsync_DeveOrdenarPrioridadeSemanticamenteNasDuasDirecoes()
    {
        await using var database = await CriarBancoAsync();
        var media = NovaTarefa("Média", SituacoesTarefa.Pendente, PrioridadesTarefa.Media);
        var baixa = NovaTarefa("Baixa", SituacoesTarefa.Pendente, PrioridadesTarefa.Baixa);
        var alta = NovaTarefa("Alta", SituacoesTarefa.Pendente, PrioridadesTarefa.Alta);

        await SalvarTarefasAsync(database, media, baixa, alta);

        await using var context = database.CreateContext();
        var repository = new TarefaRepository(context);
        var crescente = await repository.ListarAtivasAsync(Consulta(ordenarPor: CampoOrdenacaoTarefa.Prioridade, direcao: DirecaoOrdenacao.Asc));
        var decrescente = await repository.ListarAtivasAsync(Consulta(ordenarPor: CampoOrdenacaoTarefa.Prioridade, direcao: DirecaoOrdenacao.Desc));

        Assert.Equal([baixa.Id, media.Id, alta.Id], crescente.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal([alta.Id, media.Id, baixa.Id], decrescente.Itens.Select(tarefa => tarefa.Id));
    }

    [Fact]
    public async Task ListarAtivasAsync_DeveOrdenarAntesDePaginar()
    {
        await using var database = await CriarBancoAsync();
        var c = NovaTarefa("C", SituacoesTarefa.Pendente);
        var a = NovaTarefa("A", SituacoesTarefa.Pendente);
        var d = NovaTarefa("D", SituacoesTarefa.Pendente);
        var b = NovaTarefa("B", SituacoesTarefa.Pendente);

        await SalvarTarefasAsync(database, c, a, d, b);

        await using var context = database.CreateContext();
        var repository = new TarefaRepository(context);
        var primeiraPagina = await repository.ListarAtivasAsync(Consulta(ordenarPor: CampoOrdenacaoTarefa.Descricao, direcao: DirecaoOrdenacao.Asc, tamanhoPagina: 2));
        var segundaPagina = await repository.ListarAtivasAsync(Consulta(ordenarPor: CampoOrdenacaoTarefa.Descricao, direcao: DirecaoOrdenacao.Asc, pagina: 2, tamanhoPagina: 2));

        Assert.Equal(4, primeiraPagina.TotalItens);
        Assert.Equal([a.Id, b.Id], primeiraPagina.Itens.Select(tarefa => tarefa.Id));
        Assert.Equal([c.Id, d.Id], segundaPagina.Itens.Select(tarefa => tarefa.Id));
    }

    [Fact]
    public async Task ListarHistoricoAsync_DevePersistirValoresEListarDoMaisRecenteParaOMaisAntigo()
    {
        await using var database = await CriarBancoAsync();
        var tarefa = NovaTarefa("Com histórico", SituacoesTarefa.Pendente);
        var dezHoras = new HistoricoTarefa { Tarefa = tarefa, Tipo = TiposHistoricoTarefa.Criacao, CriadoEm = CriadaEm.AddHours(2) };
        var oitoHoras = new HistoricoTarefa { Tarefa = tarefa, Tipo = TiposHistoricoTarefa.AlteracaoDescricao, Campo = "Descricao", ValorAnterior = "Antes", ValorNovo = "Depois", CriadoEm = CriadaEm };
        var dozeHoras = new HistoricoTarefa { Tarefa = tarefa, Tipo = TiposHistoricoTarefa.AlteracaoPrioridade, CriadoEm = CriadaEm.AddHours(4) };

        await using (var contextoGravacao = database.CreateContext())
        {
            var repository = new TarefaRepository(contextoGravacao);
            repository.Adicionar(tarefa);
            repository.AdicionarHistorico(dezHoras);
            repository.AdicionarHistorico(oitoHoras);
            repository.AdicionarHistorico(dozeHoras);
            await repository.SalvarAlteracoesAsync();
        }

        await using var contextoLeitura = database.CreateContext();
        var historicos = await new TarefaRepository(contextoLeitura).ListarHistoricoAsync(tarefa.Id);

        Assert.Equal([dozeHoras.Id, dezHoras.Id, oitoHoras.Id], historicos.Select(historico => historico.Id));
        var alteracao = Assert.Single(historicos, historico => historico.Tipo == TiposHistoricoTarefa.AlteracaoDescricao);
        Assert.Equal(tarefa.Id, alteracao.TarefaId);
        Assert.Equal("Antes", alteracao.ValorAnterior);
        Assert.Equal("Depois", alteracao.ValorNovo);
        Assert.Equal(CriadaEm, alteracao.CriadoEm);
    }

    [Fact]
    public async Task RemoverESalvarAlteracoesAsync_DeveExcluirHistoricosPorCascadeComForeignKeysAtivas()
    {
        await using var database = await CriarBancoAsync();
        Assert.True(await database.ForeignKeysEnabledAsync());
        var tarefa = NovaTarefa("Excluir permanentemente", SituacoesTarefa.Pendente, excluidaEm: CriadaEm.AddHours(1));

        await using (var contextoGravacao = database.CreateContext())
        {
            contextoGravacao.AddRange(
                new HistoricoTarefa { Tarefa = tarefa, Tipo = TiposHistoricoTarefa.Criacao, CriadoEm = CriadaEm },
                new HistoricoTarefa { Tarefa = tarefa, Tipo = TiposHistoricoTarefa.Exclusao, CriadoEm = CriadaEm.AddHours(1) },
                new HistoricoTarefa { Tarefa = tarefa, Tipo = TiposHistoricoTarefa.Restauracao, CriadoEm = CriadaEm.AddHours(2) });
            await contextoGravacao.SaveChangesAsync();
        }

        await using (var contextoRemocao = database.CreateContext())
        {
            var repository = new TarefaRepository(contextoRemocao);
            var encontrada = await repository.BuscarIncluindoExcluidasPorIdAsync(tarefa.Id);
            repository.Remover(Assert.IsType<Tarefa>(encontrada));
            await repository.SalvarAlteracoesAsync();
        }

        await using var verificacao = database.CreateContext();
        Assert.Null(await verificacao.Tarefas.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == tarefa.Id));
        Assert.Empty(await verificacao.HistoricosTarefas.IgnoreQueryFilters().Where(item => item.TarefaId == tarefa.Id).ToListAsync());
    }

    [Fact]
    public async Task Observacoes_DevemPersistirSerLidasEPermanecerNulasNoSQLite()
    {
        await using var database = await CriarBancoAsync();
        var comObservacoes = NovaTarefa("Com observações", SituacoesTarefa.Pendente);
        comObservacoes.Observacoes = "Linha um\nLinha dois";
        var semObservacoes = NovaTarefa("Sem observações", SituacoesTarefa.Pendente);

        await SalvarTarefasAsync(database, comObservacoes, semObservacoes);

        await using var context = database.CreateContext();
        var repository = new TarefaRepository(context);
        var carregada = await repository.BuscarAtivaPorIdAsync(comObservacoes.Id);
        var carregadaSemObservacoes = await repository.BuscarAtivaPorIdAsync(semObservacoes.Id);

        Assert.Equal("Linha um\nLinha dois", carregada?.Observacoes);
        Assert.Null(carregadaSemObservacoes?.Observacoes);
    }

    private static async Task<SqliteTestDatabase> CriarBancoAsync()
    {
        var database = new SqliteTestDatabase();
        await database.InitializeAsync();
        return database;
    }

    private static async Task SalvarTarefasAsync(SqliteTestDatabase database, params Tarefa[] tarefas)
    {
        await using var context = database.CreateContext();
        context.Tarefas.AddRange(tarefas);
        await context.SaveChangesAsync();
    }

    private static Tarefa NovaTarefa(
        string descricao,
        string situacao,
        string prioridade = PrioridadesTarefa.Media,
        DateOnly? dataVencimento = null,
        DateTime? excluidaEm = null)
    {
        return new Tarefa
        {
            Descricao = descricao,
            Situacao = situacao,
            Prioridade = prioridade,
            DataVencimento = dataVencimento,
            CriadaEm = CriadaEm,
            SituacaoAlteradaEm = CriadaEm,
            ConcluidaEm = situacao == SituacoesTarefa.Concluida ? CriadaEm : null,
            ExcluidaEm = excluidaEm
        };
    }

    private static ConsultaTarefas Consulta(
        string? busca = null,
        string? situacao = null,
        string? prioridade = null,
        FiltroPrazoTarefa prazo = FiltroPrazoTarefa.Todos,
        CampoOrdenacaoTarefa ordenarPor = CampoOrdenacaoTarefa.Descricao,
        DirecaoOrdenacao direcao = DirecaoOrdenacao.Asc,
        int pagina = 1,
        int tamanhoPagina = 20)
    {
        return new ConsultaTarefas
        {
            Busca = busca,
            Situacao = situacao,
            Prioridade = prioridade,
            Prazo = prazo,
            Hoje = Hoje,
            OrdenarPor = ordenarPor,
            Direcao = direcao,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };
    }
}
