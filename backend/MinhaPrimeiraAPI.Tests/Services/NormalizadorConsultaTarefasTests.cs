using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.Models;
using MinhaPrimeiraAPI.Repositories;
using MinhaPrimeiraAPI.Services;

namespace MinhaPrimeiraAPI.Tests.Services;

public class NormalizadorConsultaTarefasTests
{
    private static readonly DateOnly Hoje = new(2030, 1, 2);

    [Fact]
    public void Normalizar_ConsultaPadrao_DeveAplicarDefaultsEPreservarDataDeNegocio()
    {
        var resultado = NormalizadorConsultaTarefas.Normalizar(new ConsultaTarefasRequest(), Hoje);

        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(10, resultado.TamanhoPagina);
        Assert.Equal(CampoOrdenacaoTarefa.UltimaAtualizacao, resultado.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Desc, resultado.Direcao);
        Assert.Equal(FiltroPrazoTarefa.Todos, resultado.Prazo);
        Assert.Equal(Hoje, resultado.Hoje);
    }

    [Fact]
    public void Normalizar_ConsultaValida_DeveCanonicalizarFiltrosEOrdenacao()
    {
        var resultado = NormalizadorConsultaTarefas.Normalizar(new ConsultaTarefasRequest
        {
            Busca = "  relatório  ",
            Situacao = " pendente ",
            Prioridade = " alta ",
            Prazo = "vencidas",
            OrdenarPor = "dataVencimento",
            Direcao = "asc",
            Pagina = 2,
            TamanhoPagina = 20
        }, Hoje);

        Assert.Equal("relatório", resultado.Busca);
        Assert.Equal(SituacoesTarefa.Pendente, resultado.Situacao);
        Assert.Equal(PrioridadesTarefa.Alta, resultado.Prioridade);
        Assert.Equal(FiltroPrazoTarefa.Vencidas, resultado.Prazo);
        Assert.Equal(CampoOrdenacaoTarefa.DataVencimento, resultado.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Asc, resultado.Direcao);
        Assert.Equal(2, resultado.Pagina);
        Assert.Equal(20, resultado.TamanhoPagina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Normalizar_PaginacaoInvalida_DeveRejeitar(int tamanhoPagina)
    {
        Assert.Throws<ArgumentException>(() => NormalizadorConsultaTarefas.Normalizar(
            new ConsultaTarefasRequest { TamanhoPagina = tamanhoPagina },
            Hoje));
    }
}
