using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;
using System.Text.Json;

namespace ProjetoTarefas.Services;

public class TarefaService : ITarefaService
{
    private const int LimiteDescricao = 200;
    private const int LimiteObservacoes = 4000;
    private static readonly TimeZoneInfo FusoHorarioNegocio = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    private readonly ITarefaRepository _tarefaRepository;
    private readonly ILogger<TarefaService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IEtiquetaRepository _etiquetaRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IUsuarioAtual _usuarioAtual;

    public TarefaService(
        ITarefaRepository tarefaRepository,
        ILogger<TarefaService> logger,
        TimeProvider timeProvider,
        IEtiquetaRepository etiquetaRepository,
        IProjetoRepository projetoRepository,
        IUsuarioAtual usuarioAtual)
    {
        _tarefaRepository = tarefaRepository;
        _logger = logger;
        _timeProvider = timeProvider;
        _etiquetaRepository = etiquetaRepository;
        _projetoRepository = projetoRepository;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<TarefasPaginadasResponse> ListarAsync(
        ConsultaTarefasRequest consulta,
        CancellationToken cancellationToken = default)
    {
        var consultaNormalizada = NormalizadorConsultaTarefas.Normalizar(consulta, ObterDataAtualNegocio());
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
                "Tarefa não encontrada para consulta do histórico. TarefaId={TarefaId}",
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
            UsuarioId = _usuarioAtual.Id,
            Descricao = NormalizarDescricao(novaTarefa.Descricao),
            Observacoes = NormalizarObservacoes(novaTarefa.Observacoes),
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

        tarefa.Etiquetas = await ObterEtiquetasAsync(novaTarefa.EtiquetaIds, cancellationToken);
        tarefa.Projeto = await ObterProjetoAsync(novaTarefa.ProjetoId, cancellationToken);

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

        var novaDescricao = NormalizarDescricao(dadosAtualizados.Descricao);

        var novasObservacoes = NormalizarObservacoes(dadosAtualizados.Observacoes);

        var novaSituacao = NormalizadorConsultaTarefas.NormalizarSituacao(dadosAtualizados.Situacao);

        var novaPrioridade = NormalizarPrioridadeAtualizacao(dadosAtualizados.Prioridade, tarefaEncontrada.Prioridade);

        var descricaoAlterada =
            !string.Equals(
                tarefaEncontrada.Descricao,
                novaDescricao,
                StringComparison.Ordinal
            );

        var observacoesAlteradas = !string.Equals(
            tarefaEncontrada.Observacoes,
            novasObservacoes,
            StringComparison.Ordinal);

        var situacaoAlterada =
            !string.Equals(
                tarefaEncontrada.Situacao,
                novaSituacao,
                StringComparison.Ordinal
            );

        var prioridadeAlterada = !string.Equals(tarefaEncontrada.Prioridade, novaPrioridade, StringComparison.Ordinal);
        var dataVencimentoAlterada = tarefaEncontrada.DataVencimento != dadosAtualizados.DataVencimento;
        var novoProjeto = await ObterProjetoAsync(dadosAtualizados.ProjetoId, cancellationToken);
        var projetoAlterado = tarefaEncontrada.ProjetoId != novoProjeto?.Id;
        var novasEtiquetas = await ObterEtiquetasAsync(dadosAtualizados.EtiquetaIds, cancellationToken);
        var etiquetasAnteriores = FormatarEtiquetas(tarefaEncontrada.Etiquetas);
        var etiquetasNovas = FormatarEtiquetas(novasEtiquetas);
        var etiquetasAlteradas = !string.Equals(etiquetasAnteriores, etiquetasNovas, StringComparison.Ordinal);

        if (!descricaoAlterada && !observacoesAlteradas && !situacaoAlterada && !prioridadeAlterada && !dataVencimentoAlterada && !projetoAlterado && !etiquetasAlteradas)
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
        var observacoesAnteriores = tarefaEncontrada.Observacoes;
        var prioridadeAnterior = tarefaEncontrada.Prioridade;
        var dataVencimentoAnterior = tarefaEncontrada.DataVencimento;
        var nomeProjetoAnterior = tarefaEncontrada.Projeto?.Nome;

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

        if (observacoesAlteradas)
        {
            tarefaEncontrada.Observacoes = novasObservacoes;
            _tarefaRepository.AdicionarHistorico(CriarHistorico(
                tarefaEncontrada,
                TiposHistoricoTarefa.AlteracaoObservacoes,
                agora,
                "Observacoes",
                observacoesAnteriores,
                novasObservacoes));
        }

        if (prioridadeAlterada)
        {
            tarefaEncontrada.Prioridade = novaPrioridade;
            _tarefaRepository.AdicionarHistorico(CriarHistorico(
                tarefaEncontrada,
                TiposHistoricoTarefa.AlteracaoPrioridade,
                agora,
                "Prioridade",
                prioridadeAnterior,
                novaPrioridade));
        }

        if (dataVencimentoAlterada)
        {
            tarefaEncontrada.DataVencimento = dadosAtualizados.DataVencimento;
            _tarefaRepository.AdicionarHistorico(CriarHistorico(
                tarefaEncontrada,
                TiposHistoricoTarefa.AlteracaoDataVencimento,
                agora,
                "DataVencimento",
                FormatarDataVencimento(dataVencimentoAnterior),
                FormatarDataVencimento(dadosAtualizados.DataVencimento)));
        }

        if (projetoAlterado)
        {
            tarefaEncontrada.Projeto = novoProjeto;
            tarefaEncontrada.ProjetoId = novoProjeto?.Id;
            _tarefaRepository.AdicionarHistorico(CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.AlteracaoProjeto, agora, "Projeto", nomeProjetoAnterior, novoProjeto?.Nome));
        }

        if (situacaoAlterada)
        {
            AplicarTransicaoSituacao(tarefaEncontrada, novaSituacao, agora);
        }

        if (etiquetasAlteradas)
        {
            tarefaEncontrada.Etiquetas = novasEtiquetas;
            _tarefaRepository.AdicionarHistorico(CriarHistorico(tarefaEncontrada, TiposHistoricoTarefa.AlteracaoEtiquetas, agora, "Etiquetas", etiquetasAnteriores, etiquetasNovas));
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

    private static string NormalizarSituacaoCriacao(
        string? situacao)
    {
        return string.IsNullOrWhiteSpace(situacao)
            ? SituacoesTarefa.Pendente
            : NormalizadorConsultaTarefas.NormalizarSituacao(situacao);
    }

    private static string NormalizarPrioridadeCriacao(string? prioridade)
    {
        return string.IsNullOrWhiteSpace(prioridade) ? PrioridadesTarefa.Media : NormalizadorConsultaTarefas.NormalizarPrioridade(prioridade);
    }

    private static string NormalizarPrioridadeAtualizacao(string? prioridade, string prioridadeAtual)
    {
        return string.IsNullOrWhiteSpace(prioridade) ? prioridadeAtual : NormalizadorConsultaTarefas.NormalizarPrioridade(prioridade);
    }

    private DateOnly ObterDataAtualNegocio()
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), FusoHorarioNegocio).DateTime);
    }

    private static string? FormatarDataVencimento(DateOnly? dataVencimento)
    {
        return dataVencimento?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string? NormalizarObservacoes(string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(observacoes))
        {
            return null;
        }

        var resultado = observacoes.Trim();

        if (resultado.Length > LimiteObservacoes)
        {
            throw new ArgumentException(
                $"As observações da tarefa devem ter no máximo {LimiteObservacoes} caracteres.",
                nameof(observacoes)
            );
        }

        return resultado;
    }

    private static string NormalizarDescricao(string? descricao)
    {
        var resultado = descricao?.Trim();

        if (string.IsNullOrWhiteSpace(resultado))
        {
            throw new ArgumentException("A descrição da tarefa é obrigatória.", nameof(descricao));
        }

        if (resultado.Length > LimiteDescricao)
        {
            throw new ArgumentException(
                $"A descrição da tarefa deve ter no máximo {LimiteDescricao} caracteres.",
                nameof(descricao)
            );
        }

        return resultado;
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

    private void AplicarTransicaoSituacao(Tarefa tarefa, string novaSituacao, DateTime agora)
    {
        var situacaoAnterior = tarefa.Situacao;

        if (string.Equals(situacaoAnterior, novaSituacao, StringComparison.Ordinal))
        {
            return;
        }

        if (!TransicaoSituacaoEhPermitida(situacaoAnterior, novaSituacao))
        {
            throw new ArgumentException("A transição de situação da tarefa é inválida.", nameof(novaSituacao));
        }

        tarefa.Situacao = novaSituacao;
        tarefa.SituacaoAlteradaEm = agora;
        tarefa.ConcluidaEm = EstaConcluida(novaSituacao) ? agora : null;

        _tarefaRepository.AdicionarHistorico(CriarHistorico(
            tarefa,
            ObterTipoHistoricoSituacao(situacaoAnterior, novaSituacao),
            agora,
            "Situacao",
            situacaoAnterior,
            novaSituacao));
    }

    private static bool TransicaoSituacaoEhPermitida(string origem, string destino)
    {
        return (origem, destino) switch
        {
            (SituacoesTarefa.Pendente, SituacoesTarefa.EmAndamento) => true,
            (SituacoesTarefa.Pendente, SituacoesTarefa.Concluida) => true,
            (SituacoesTarefa.EmAndamento, SituacoesTarefa.Pendente) => true,
            (SituacoesTarefa.EmAndamento, SituacoesTarefa.Concluida) => true,
            (SituacoesTarefa.Concluida, SituacoesTarefa.Pendente) => true,
            (SituacoesTarefa.Concluida, SituacoesTarefa.EmAndamento) => true,
            _ => false
        };
    }

    private static string ObterTipoHistoricoSituacao(string origem, string destino)
    {
        if (!EstaConcluida(origem) && EstaConcluida(destino))
        {
            return TiposHistoricoTarefa.Conclusao;
        }

        return EstaConcluida(origem) && !EstaConcluida(destino)
            ? TiposHistoricoTarefa.Reabertura
            : TiposHistoricoTarefa.AlteracaoSituacao;
    }

    private static TarefaResponse MapearParaResponse(
        Tarefa tarefa)
    {
        return new TarefaResponse
        {
            Id = tarefa.Id,
            Descricao = tarefa.Descricao,
            Observacoes = tarefa.Observacoes,
            Situacao = tarefa.Situacao,
            Prioridade = tarefa.Prioridade,
            DataVencimento = tarefa.DataVencimento,
            CriadaEm = tarefa.CriadaEm,
            ModificadaEm = tarefa.ModificadaEm,
            SituacaoAlteradaEm = tarefa.SituacaoAlteradaEm,
            ConcluidaEm = tarefa.ConcluidaEm,
            ExcluidaEm = tarefa.ExcluidaEm,
            Projeto = tarefa.Projeto is null ? null : new ProjetoResponse { Id = tarefa.Projeto.Id, Nome = tarefa.Projeto.Nome },
            Etiquetas = tarefa.Etiquetas.OrderBy(etiqueta => etiqueta.Nome).ThenBy(etiqueta => etiqueta.Id).Select(etiqueta => new EtiquetaResponse { Id = etiqueta.Id, Nome = etiqueta.Nome }).ToList()
        };
    }

    private async Task<List<Etiqueta>> ObterEtiquetasAsync(IEnumerable<int>? idsRecebidos, CancellationToken cancellationToken)
    {
        var origem = idsRecebidos ?? [];
        if (origem.Any(id => id <= 0)) throw new ArgumentException("Os IDs das etiquetas devem ser maiores que zero.", nameof(idsRecebidos));
        var ids = origem.Distinct().ToList();
        if (ids.Count == 0) return [];
        var etiquetas = await _etiquetaRepository.BuscarPorIdsAsync(ids, cancellationToken);
        if (etiquetas.Count != ids.Count) throw new ArgumentException("Uma ou mais etiquetas informadas não existem.", nameof(idsRecebidos));
        return etiquetas.OrderBy(etiqueta => etiqueta.Nome).ThenBy(etiqueta => etiqueta.Id).ToList();
    }

    private async Task<Projeto?> ObterProjetoAsync(int? projetoId, CancellationToken cancellationToken)
    {
        if (projetoId is null) return null;
        if (projetoId <= 0) throw new ArgumentException("O ID do projeto deve ser maior que zero.", nameof(projetoId));
        return await _projetoRepository.BuscarPorIdAsync(projetoId.Value, rastrearAlteracoes: true, cancellationToken: cancellationToken)
            ?? throw new ArgumentException("O projeto informado não existe.", nameof(projetoId));
    }

    private static string FormatarEtiquetas(IEnumerable<Etiqueta> etiquetas)
    {
        var nomes = etiquetas
            .OrderBy(etiqueta => etiqueta.Nome)
            .ThenBy(etiqueta => etiqueta.Id)
            .Select(etiqueta => etiqueta.Nome);

        return JsonSerializer.Serialize(nomes);
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
