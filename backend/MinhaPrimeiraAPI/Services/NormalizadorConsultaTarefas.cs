using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;

namespace ProjetoTarefas.Services;

public static class NormalizadorConsultaTarefas
{
    private const int PaginaPadrao = 1;
    private const int TamanhoPaginaPadrao = 10;
    private const int TamanhoPaginaMaximo = 100;

    public static ConsultaTarefas Normalizar(ConsultaTarefasRequest consulta, DateOnly hoje)
    {
        var pagina = consulta.Pagina ?? PaginaPadrao;
        var tamanhoPagina = consulta.TamanhoPagina ?? TamanhoPaginaPadrao;

        if (pagina < 1)
        {
            throw new ArgumentException("A página deve ser maior que zero.", nameof(consulta.Pagina));
        }

        if (tamanhoPagina is < 1 or > TamanhoPaginaMaximo)
        {
            throw new ArgumentException("O tamanho da página deve estar entre 1 e 100.", nameof(consulta.TamanhoPagina));
        }

        return new ConsultaTarefas
        {
            Busca = string.IsNullOrWhiteSpace(consulta.Busca) ? null : consulta.Busca.Trim(),
            Situacao = string.IsNullOrWhiteSpace(consulta.Situacao) ? null : NormalizarSituacao(consulta.Situacao),
            Prioridade = string.IsNullOrWhiteSpace(consulta.Prioridade) ? null : NormalizarPrioridade(consulta.Prioridade),
            EtiquetaId = NormalizarEtiquetaId(consulta.EtiquetaId),
            ProjetoId = NormalizarProjetoId(consulta.ProjetoId),
            Prazo = NormalizarPrazo(consulta.Prazo),
            Hoje = hoje,
            OrdenarPor = NormalizarOrdenacao(consulta.OrdenarPor),
            Direcao = NormalizarDirecao(consulta.Direcao),
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };
    }

    private static int? NormalizarEtiquetaId(int? etiquetaId)
    {
        if (etiquetaId is null) return null;
        if (etiquetaId <= 0) throw new ArgumentException("O ID da etiqueta deve ser maior que zero.", nameof(etiquetaId));
        return etiquetaId;
    }

    private static int? NormalizarProjetoId(int? projetoId)
    {
        if (projetoId is null) return null;
        if (projetoId <= 0) throw new ArgumentException("O ID do projeto deve ser maior que zero.", nameof(projetoId));
        return projetoId;
    }

    public static string NormalizarSituacao(string situacao)
    {
        var situacaoNormalizada = situacao.Trim();

        if (string.Equals(situacaoNormalizada, SituacoesTarefa.Pendente, StringComparison.OrdinalIgnoreCase)) return SituacoesTarefa.Pendente;
        if (string.Equals(situacaoNormalizada, SituacoesTarefa.EmAndamento, StringComparison.OrdinalIgnoreCase)) return SituacoesTarefa.EmAndamento;
        if (string.Equals(situacaoNormalizada, SituacoesTarefa.Concluida, StringComparison.OrdinalIgnoreCase)) return SituacoesTarefa.Concluida;

        throw new ArgumentException("A situação da tarefa é inválida.", nameof(situacao));
    }

    public static string NormalizarPrioridade(string prioridade)
    {
        return prioridade.Trim().ToLowerInvariant() switch
        {
            "baixa" => PrioridadesTarefa.Baixa,
            "media" => PrioridadesTarefa.Media,
            "alta" => PrioridadesTarefa.Alta,
            _ => throw new ArgumentException("A prioridade da tarefa é inválida.", nameof(prioridade))
        };
    }

    private static CampoOrdenacaoTarefa NormalizarOrdenacao(string? ordenarPor)
    {
        if (string.IsNullOrWhiteSpace(ordenarPor)) return CampoOrdenacaoTarefa.UltimaAtualizacao;

        return ordenarPor.Trim().ToLowerInvariant() switch
        {
            "descricao" => CampoOrdenacaoTarefa.Descricao,
            "situacao" => CampoOrdenacaoTarefa.Situacao,
            "prioridade" => CampoOrdenacaoTarefa.Prioridade,
            "datavencimento" => CampoOrdenacaoTarefa.DataVencimento,
            "ultimaatualizacao" => CampoOrdenacaoTarefa.UltimaAtualizacao,
            _ => throw new ArgumentException("O campo de ordenação é inválido.", nameof(ordenarPor))
        };
    }

    private static DirecaoOrdenacao NormalizarDirecao(string? direcao)
    {
        if (string.IsNullOrWhiteSpace(direcao)) return DirecaoOrdenacao.Desc;

        return direcao.Trim().ToLowerInvariant() switch
        {
            "asc" => DirecaoOrdenacao.Asc,
            "desc" => DirecaoOrdenacao.Desc,
            _ => throw new ArgumentException("A direção de ordenação é inválida.", nameof(direcao))
        };
    }

    private static FiltroPrazoTarefa NormalizarPrazo(string? prazo)
    {
        if (string.IsNullOrWhiteSpace(prazo)) return FiltroPrazoTarefa.Todos;

        return prazo.Trim().ToLowerInvariant() switch
        {
            "vencidas" => FiltroPrazoTarefa.Vencidas,
            "vencemhoje" => FiltroPrazoTarefa.VencemHoje,
            "proximas" => FiltroPrazoTarefa.Proximas,
            "semvencimento" => FiltroPrazoTarefa.SemVencimento,
            _ => throw new ArgumentException("O filtro de prazo é inválido.", nameof(prazo))
        };
    }
}
