using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.DTOs.Responses;
using MinhaPrimeiraAPI.Models;
using MinhaPrimeiraAPI.Repositories;

namespace MinhaPrimeiraAPI.Services;

public class TarefaService : ITarefaService
{
    private static readonly TimeZoneInfo FusoHorarioNegocio = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    private readonly ITarefaRepository _tarefaRepository;
    private readonly ILogger<TarefaService> _logger;
    private readonly TimeProvider _timeProvider;

    public TarefaService(
        ITarefaRepository tarefaRepository,
        ILogger<TarefaService> logger,
        TimeProvider timeProvider)
    {
        _tarefaRepository = tarefaRepository;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<TarefasPaginadasResponse> ListarAsync(
        ConsultaTarefasRequest consulta,
        CancellationToken cancellationToken = default)
    {
        var consultaNormalizada = NormalizarConsulta(consulta);
        var resultado = await _tarefaRepository.ListarAtivasAsync(consultaNormalizada, cancellationToken);

        _logger.LogInformation(
            "Listagem de tarefas concluída. Quantidade={Quantidade}",
            resultado.TotalItens
        );

        return new TarefasPaginadasResponse
        {
            Itens = resultado.Itens.Select(MapearParaResponse).ToList(),
            PaginaAtual = consultaNormalizada.Pagina,
            TamanhoPagina = consultaNormalizada.TamanhoPagina,
            TotalItens = resultado.TotalItens,
            TotalPaginas = (int)Math.Ceiling(
                resultado.TotalItens / (double)consultaNormalizada.TamanhoPagina)
        };
    }

    public async Task<ResumoTarefasResponse> ObterResumoAsync(CancellationToken cancellationToken = default)
    {
        var resumo = await _tarefaRepository.ObterResumoAtivasAsync(cancellationToken);

        return new ResumoTarefasResponse
        {
            Total = resumo.Total,
            Pendentes = resumo.Pendentes,
            EmAndamento = resumo.EmAndamento,
            Concluidas = resumo.Concluidas
        };
    }

    public async Task<List<TarefaResponse>> ListarExcluidasAsync(CancellationToken cancellationToken = default)
    {
        var tarefasEncontradas =
            await _tarefaRepository.ListarExcluidasAsync(cancellationToken);

        _logger.LogInformation(
            "Listagem de tarefas excluídas concluída. Quantidade={Quantidade}",
            tarefasEncontradas.Count
        );

        return tarefasEncontradas
            .Select(MapearParaResponse)
            .ToList();
    }

    public async Task<TarefaResponse?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tarefaEncontrada =
            await _tarefaRepository.BuscarAtivaPorIdAsync(id, cancellationToken: cancellationToken);

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada. TarefaId={TarefaId}",
                id
            );

            return null;
        }

        return MapearParaResponse(tarefaEncontrada);
    }

    public async Task<List<HistoricoTarefaResponse>?> ListarHistoricoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var tarefaEncontrada = await _tarefaRepository
            .BuscarIncluindoExcluidasPorIdAsync(id, cancellationToken);

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa nÃ£o encontrada para consulta do histÃ³rico. TarefaId={TarefaId}",
                id
            );

            return null;
        }

        var historico = await _tarefaRepository.ListarHistoricoAsync(id, cancellationToken);

        return historico.Select(MapearHistoricoParaResponse).ToList();
    }

    public async Task<TarefaResponse> CriarAsync(
        CriarTarefaRequest novaTarefa,
        CancellationToken cancellationToken = default)
    {
        var agora = _timeProvider.GetUtcNow().UtcDateTime;

        var situacao =
            NormalizarSituacaoCriacao(
                novaTarefa.Situacao
            );

        var prioridade = NormalizarPrioridadeCriacao(novaTarefa.Prioridade);

        var tarefa = new Tarefa
        {
            Descricao = novaTarefa.Descricao.Trim(),
            Situacao = situacao,
            Prioridade = prioridade,
            DataVencimento = novaTarefa.DataVencimento,
            CriadaEm = agora,
            ModificadaEm = null,
            SituacaoAlteradaEm = agora,
            ConcluidaEm = EstaConcluida(situacao)
                ? agora
                : null,
            ExcluidaEm = null
        };

        _tarefaRepository.Adicionar(tarefa);
        _tarefaRepository.AdicionarHistorico(CriarHistorico(tarefa, TiposHistoricoTarefa.Criacao, agora));
        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarefa criada. TarefaId={TarefaId}. CriadaEm={CriadaEm}",
            tarefa.Id,
            tarefa.CriadaEm
        );

        return MapearParaResponse(tarefa);
    }

    public async Task<TarefaResponse?> AtualizarAsync(
        int id,
        AtualizarTarefaRequest dadosAtualizados,
        CancellationToken cancellationToken = default)
    {
        var tarefaEncontrada =
            await _tarefaRepository.BuscarAtivaPorIdAsync(
                id,
                rastrearAlteracoes: true,
                cancellationToken: cancellationToken
            );

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada para atualização. TarefaId={TarefaId}",
                id
            );

            return null;
        }

        var novaDescricao =
            dadosAtualizados.Descricao.Trim();

        var novaSituacao =
            NormalizarSituacao(dadosAtualizados.Situacao);

        var novaPrioridade = NormalizarPrioridadeAtualizacao(dadosAtualizados.Prioridade, tarefaEncontrada.Prioridade);

        var descricaoAlterada =
            !string.Equals(
                tarefaEncontrada.Descricao,
                novaDescricao,
                StringComparison.Ordinal
            );

        var situacaoAlterada =
            !string.Equals(
                tarefaEncontrada.Situacao,
                novaSituacao,
                StringComparison.Ordinal
            );

        var prioridadeAlterada = !string.Equals(tarefaEncontrada.Prioridade, novaPrioridade, StringComparison.Ordinal);
        var dataVencimentoAlterada = tarefaEncontrada.DataVencimento != dadosAtualizados.DataVencimento;

        if (!descricaoAlterada && !situacaoAlterada && !prioridadeAlterada && !dataVencimentoAlterada)
        {
            _logger.LogInformation(
                "Atualização ignorada porque não houve alterações. TarefaId={TarefaId}",
                id
            );

            return MapearParaResponse(
                tarefaEncontrada
            );
        }

        var agora = _timeProvider.GetUtcNow().UtcDateTime;
        var descricaoAnterior = tarefaEncontrada.Descricao;
        var situacaoAnterior = tarefaEncontrada.Situacao;
        var prioridadeAnterior = tarefaEncontrada.Prioridade;
        var dataVencimentoAnterior = tarefaEncontrada.DataVencimento;

        if (descricaoAlterada)
        {
            tarefaEncontrada.Descricao =
                novaDescricao;

            _tarefaRepository.AdicionarHistorico(
                CriarHistorico(
                    tarefaEncontrada,
                    TiposHistoricoTarefa.AlteracaoDescricao,
                    agora,
                    campo: "Descricao",
                    valorAnterior: descricaoAnterior,
                    valorNovo: novaDescricao
                )
            );
        }

        if (prioridadeAlterada)
        {
            tarefaEncontrada.Prioridade = novaPrioridade;
            _tarefaRepository.AdicionarHistorico(CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.AlteracaoPrioridade, agora, "Prioridade", prioridadeAnterior, novaPrioridade));
        }

        if (dataVencimentoAlterada)
        {
            tarefaEncontrada.DataVencimento = dadosAtualizados.DataVencimento;
            _tarefaRepository.AdicionarHistorico(CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.AlteracaoDataVencimento, agora, "DataVencimento", FormatarDataVencimento(dataVencimentoAnterior), FormatarDataVencimento(dadosAtualizados.DataVencimento)));
        }

        if (situacaoAlterada)
        {
            tarefaEncontrada.Situacao =
                novaSituacao;

            tarefaEncontrada.SituacaoAlteradaEm =
                agora;

            tarefaEncontrada.ConcluidaEm =
                EstaConcluida(novaSituacao)
                    ? agora
                    : null;

            if (!EstaConcluida(situacaoAnterior) && EstaConcluida(novaSituacao))
            {
                _tarefaRepository.AdicionarHistorico(
                    CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.Conclusao, agora)
                );
            }
            else if (EstaConcluida(situacaoAnterior) && !EstaConcluida(novaSituacao))
            {
                _tarefaRepository.AdicionarHistorico(
                    CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.Reabertura, agora)
                );
            }
        }

        tarefaEncontrada.ModificadaEm = agora;

        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarefa atualizada. TarefaId={TarefaId}. DescricaoAlterada={DescricaoAlterada}. SituacaoAlterada={SituacaoAlterada}. ModificadaEm={ModificadaEm}",
            id,
            descricaoAlterada,
            situacaoAlterada,
            tarefaEncontrada.ModificadaEm
        );

        return MapearParaResponse(
            tarefaEncontrada
        );
    }

    public async Task<bool> ExcluirLogicamenteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tarefaEncontrada =
            await _tarefaRepository.BuscarAtivaPorIdAsync(
                id,
                rastrearAlteracoes: true,
                cancellationToken: cancellationToken
            );

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada para exclusão lógica. TarefaId={TarefaId}",
                id
            );

            return false;
        }

        var agora = _timeProvider.GetUtcNow().UtcDateTime;
        tarefaEncontrada.ExcluidaEm = agora;
        _tarefaRepository.AdicionarHistorico(
            CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.Exclusao, agora)
        );

        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarefa excluída logicamente. TarefaId={TarefaId}. ExcluidaEm={ExcluidaEm}",
            tarefaEncontrada.Id,
            tarefaEncontrada.ExcluidaEm
        );

        return true;
    }

    public async Task<ResultadoRestauracao> RestaurarAsync(int id, CancellationToken cancellationToken = default)
    {
        var tarefaEncontrada =
            await _tarefaRepository.BuscarIncluindoExcluidasPorIdAsync(id, cancellationToken);

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada para restauração. TarefaId={TarefaId}",
                id
            );

            return ResultadoRestauracao.NaoEncontrada;
        }

        if (tarefaEncontrada.ExcluidaEm is null)
        {
            _logger.LogWarning(
                "Restauração bloqueada: tarefa já está ativa. TarefaId={TarefaId}",
                id
            );

            return ResultadoRestauracao.TarefaAtiva;
        }

        var agora = _timeProvider.GetUtcNow().UtcDateTime;
        tarefaEncontrada.ExcluidaEm = null;
        tarefaEncontrada.ModificadaEm = agora;
        _tarefaRepository.AdicionarHistorico(
            CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.Restauracao, agora)
        );

        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarefa restaurada. TarefaId={TarefaId}. ModificadaEm={ModificadaEm}",
            id,
            tarefaEncontrada.ModificadaEm
        );

        return ResultadoRestauracao.Sucesso;
    }

    public async Task<ResultadoExclusaoPermanente>
        ExcluirPermanentementeAsync(int id, CancellationToken cancellationToken = default)
    {
        var tarefaEncontrada =
            await _tarefaRepository
                .BuscarIncluindoExcluidasPorIdAsync(id, cancellationToken);

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada para exclusão permanente. TarefaId={TarefaId}",
                id
            );

            return ResultadoExclusaoPermanente.NaoEncontrada;
        }

        if (tarefaEncontrada.ExcluidaEm is null)
        {
            _logger.LogWarning(
                "Exclusão permanente bloqueada: tarefa ainda ativa. TarefaId={TarefaId}",
                id
            );

            return ResultadoExclusaoPermanente.TarefaAtiva;
        }

        _tarefaRepository.Remover(tarefaEncontrada);
        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarefa excluída permanentemente. TarefaId={TarefaId}",
            id
        );

        return ResultadoExclusaoPermanente.Sucesso;
    }

    private ConsultaTarefas NormalizarConsulta(
        ConsultaTarefasRequest consulta)
    {
        const int paginaPadrao = 1;
        const int tamanhoPaginaPadrao = 10;
        const int tamanhoPaginaMaximo = 100;

        var pagina = consulta.Pagina ?? paginaPadrao;
        var tamanhoPagina = consulta.TamanhoPagina ?? tamanhoPaginaPadrao;

        if (pagina < 1)
        {
            throw new ArgumentException("A página deve ser maior que zero.", nameof(consulta.Pagina));
        }

        if (tamanhoPagina is < 1 or > tamanhoPaginaMaximo)
        {
            throw new ArgumentException("O tamanho da página deve estar entre 1 e 100.", nameof(consulta.TamanhoPagina));
        }

        return new ConsultaTarefas
        {
            Busca = string.IsNullOrWhiteSpace(consulta.Busca) ? null : consulta.Busca.Trim(),
            Situacao = string.IsNullOrWhiteSpace(consulta.Situacao) ? null : NormalizarSituacao(consulta.Situacao),
            Prioridade = string.IsNullOrWhiteSpace(consulta.Prioridade) ? null : NormalizarPrioridade(consulta.Prioridade),
            Prazo = NormalizarPrazo(consulta.Prazo),
            Hoje = ObterDataAtualNegocio(),
            OrdenarPor = NormalizarOrdenacao(consulta.OrdenarPor),
            Direcao = NormalizarDirecao(consulta.Direcao),
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
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

    private static string NormalizarSituacaoCriacao(
        string? situacao)
    {
        return string.IsNullOrWhiteSpace(situacao)
            ? SituacoesTarefa.Pendente
            : NormalizarSituacao(situacao);
    }

    private static string NormalizarPrioridadeCriacao(string? prioridade)
    {
        return string.IsNullOrWhiteSpace(prioridade) ? PrioridadesTarefa.Media : NormalizarPrioridade(prioridade);
    }

    private static string NormalizarPrioridadeAtualizacao(string? prioridade, string prioridadeAtual)
    {
        return string.IsNullOrWhiteSpace(prioridade) ? prioridadeAtual : NormalizarPrioridade(prioridade);
    }

    private static string NormalizarPrioridade(string prioridade)
    {
        return prioridade.Trim().ToLowerInvariant() switch
        {
            "baixa" => PrioridadesTarefa.Baixa,
            "media" => PrioridadesTarefa.Media,
            "alta" => PrioridadesTarefa.Alta,
            _ => throw new ArgumentException("A prioridade da tarefa \u00e9 inv\u00e1lida.", nameof(prioridade))
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
            _ => throw new ArgumentException("O filtro de prazo \u00e9 inv\u00e1lido.", nameof(prazo))
        };
    }

    private DateOnly ObterDataAtualNegocio()
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), FusoHorarioNegocio).DateTime);
    }

    private static string? FormatarDataVencimento(DateOnly? dataVencimento)
    {
        return dataVencimento?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string NormalizarSituacao(string situacao)
    {
        var situacaoNormalizada = situacao.Trim();

        if (string.Equals(situacaoNormalizada, SituacoesTarefa.Pendente, StringComparison.OrdinalIgnoreCase))
        {
            return SituacoesTarefa.Pendente;
        }

        if (string.Equals(situacaoNormalizada, SituacoesTarefa.EmAndamento, StringComparison.OrdinalIgnoreCase))
        {
            return SituacoesTarefa.EmAndamento;
        }

        if (string.Equals(situacaoNormalizada, SituacoesTarefa.Concluida, StringComparison.OrdinalIgnoreCase))
        {
            return SituacoesTarefa.Concluida;
        }

        throw new ArgumentException(
            "A situação da tarefa é inválida.",
            nameof(situacao)
        );
    }

    private static bool EstaConcluida(
        string situacao)
    {
        return string.Equals(
            situacao,
            SituacoesTarefa.Concluida,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static TarefaResponse MapearParaResponse(
        Tarefa tarefa)
    {
        return new TarefaResponse
        {
            Id = tarefa.Id,
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao,
            Prioridade = tarefa.Prioridade,
            DataVencimento = tarefa.DataVencimento,
            CriadaEm = tarefa.CriadaEm,
            ModificadaEm = tarefa.ModificadaEm,
            SituacaoAlteradaEm = tarefa.SituacaoAlteradaEm,
            ConcluidaEm = tarefa.ConcluidaEm,
            ExcluidaEm = tarefa.ExcluidaEm
        };
    }

    private static HistoricoTarefa CriarHistorico(
        Tarefa tarefa,
        string tipo,
        DateTime criadoEm,
        string? campo = null,
        string? valorAnterior = null,
        string? valorNovo = null)
    {
        return new HistoricoTarefa
        {
            Tarefa = tarefa,
            Tipo = tipo,
            Campo = campo,
            ValorAnterior = valorAnterior,
            ValorNovo = valorNovo,
            CriadoEm = criadoEm
        };
    }

    private static HistoricoTarefaResponse MapearHistoricoParaResponse(HistoricoTarefa historico)
    {
        return new HistoricoTarefaResponse
        {
            Id = historico.Id,
            Tipo = historico.Tipo,
            Campo = historico.Campo,
            ValorAnterior = historico.ValorAnterior,
            ValorNovo = historico.ValorNovo,
            CriadoEm = historico.CriadoEm
        };
    }
}
