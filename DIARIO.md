# Diário de trabalho

Este arquivo registra, em ordem cronológica, as atividades realizadas no repositório a partir de 21/08/2026. Ele deve ser atualizado ao concluir cada tarefa ou etapa relevante, indicando o que foi alterado, como foi validado e pendências conhecidas.

## 2026-08-24

### Redesign da homepage — central de navegação

- Os fluxos de detalhes e histórico passaram a abrir modais sobre a lista de seleção, sem redirecionamento. A visualização de detalhes foi extraída para `DetalhesTarefaConteudo`, reutilizada pelo modal e pela rota direta já existente; o modal de histórico reutiliza a timeline atual.
- O acesso à grade de tarefas foi convertido de botão isolado para o quinto card da grade principal. A homepage passou a usar contêiner `xl` e padding externo mais compacto para aproveitar melhor o espaço da tela.
- Ajustada a seta de retorno dos detalhes da tarefa para voltar à homepage, em vez da grade de tarefas.
- Removida a seção de histórico da página de detalhes, eliminando a duplicidade com o fluxo dedicado de histórico. A página deixou de chamar o endpoint de auditoria nesse acesso.
- Corrigida a navegação da página de criação: o botão de voltar agora retorna à homepage. A homepage também ganhou o botão explícito `Visualizar grade de tarefas`, que mantém o acesso à listagem geral separado do fluxo de criação.
- A homepage deixou de carregar e apresentar indicadores estatísticos como conteúdo principal. Ela passou a oferecer quatro cards acessíveis e responsivos: criar tarefa, detalhes, editar e histórico.
- Criada a rota única de seleção `/tarefas/selecionar/[modo]`, com busca textual, paginação, estados de carregamento/erro/lista vazia e uma listagem reduzida de tarefas. Detalhes encaminha para a página existente, edição reutiliza `DialogoEditarTarefa` e histórico reutiliza `listarHistoricoTarefa` e `HistoricoTarefa`.
- Nenhum endpoint, banco, migration, service HTTP, tipo de contrato ou formulário foi duplicado ou alterado.
- Validações: `npm run lint` e `npm run build` concluídos sem erros; backend com build sem avisos/erros e 95/95 testes aprovados. NÃO EXECUTADO VISUALMENTE.

### Correção de runtime — carregamento de projetos

- Diagnosticado o erro de `GET /api/projetos`: o log da API registrava `SQLite Error 1: 'no such table: PROJETOS'`. O banco de desenvolvimento tinha as migrations `AdicionarObservacoesTarefa`, `AdicionarEtiquetas` e `AdicionarProjetos` pendentes, enquanto a API já executava os endpoints e consultas correspondentes.
- Aplicada a cadeia de migrations pendentes com `dotnet ef database update`, preservando o banco e seus registros; nenhum arquivo de código-fonte foi alterado.
- Validação funcional contra a API já em execução: `GET /api/projetos`, `GET /api/etiquetas` e `GET /api/tarefas` retornaram HTTP 200; projetos e etiquetas vazios e as três tarefas existentes permaneceram acessíveis.
- Validações: build do backend concluído com 0 avisos e 0 erros; testes `dotnet test --no-build` com 94/94 aprovados, 0 falhas e 0 ignorados; lint do frontend concluído sem erros.
- Limitação: `npm run build` não pôde concluir porque o `next dev` existente mantém o lock de `.next` (`Another next build process is already running`); o processo não foi encerrado. NÃO EXECUTADO VISUALMENTE.

### Diagnóstico de cadastro de tarefa com projeto

- Confirmado no log do `POST /api/tarefas` o erro SQLite `UNIQUE constraint failed: PROJETOS.ID`. Ao criar tarefa com projeto, `ProjetoRepository.BuscarPorIdAsync` retorna a entidade sem tracking; em seguida, `TarefaRepository.Adicionar` adiciona o grafo inteiro e o EF tenta inserir novamente o projeto já existente.
- A criação de tarefa sem projeto não é afetada. A correção indicada é associar o projeto já rastreado ou definir apenas a FK `ProjetoId` depois da validação, preservando a resposta e o histórico atuais. Nenhum código ou dado foi alterado nesta etapa de diagnóstico.

### Correção de cadastro de tarefa com projeto

- `TarefaService.ObterProjetoAsync` passou a buscar o projeto com tracking quando ele é associado à criação ou atualização de uma tarefa. Assim, `TarefaRepository.Adicionar` persiste somente a nova tarefa e sua associação, sem tentar inserir novamente o projeto existente.
- Adicionado teste SQLite para criar tarefa com projeto e etiqueta existentes, verificando a preservação de um único projeto, a associação do projeto e da etiqueta na tarefa criada.
- Validação: a compilação isolada do backend e dos testes concluiu com 0 avisos e 0 erros. A execução dos testes não concluiu no executor após iniciar a descoberta, e o build padrão ficou bloqueado pela API de desenvolvimento existente, que mantém `MinhaPrimeiraAPI.exe` e `.dll` abertos; o processo não foi encerrado. O diretório temporário `TempValidation` criado para a compilação permanece pendente de limpeza porque a remoção foi bloqueada pela política do executor. NÃO EXECUTADO VISUALMENTE.

### Ajuste visual — listagem de tarefas

- Ampliado o contêiner da página de tarefas de `lg` para `xl`, reduzido o espaçamento vertical e reorganizados os filtros para preencher a largura disponível e quebrar de linha de forma responsiva.
- A tabela deixou de usar altura fixa de 610 px: ela agora tem altura calculada a partir da quantidade de linhas visíveis, com mínimo de 260 px e máximo de 620 px. A listagem deixa de exibir uma grande área vazia com poucos registros e mantém rolagem para páginas maiores.
- Validação: a tentativa de `npm run lint` não retornou resultado final no executor. `npm run build` não foi executado porque o processo `next dev` ativo mantém o lock de `.next`. NÃO EXECUTADO VISUALMENTE.

### Ajuste de escopo do diário

- A regra foi refinada: o diário passa a registrar somente tarefas, alterações, decisões e etapas relevantes.
- Atividades rotineiras ou sem impacto relevante não precisam ser incluídas; os registros devem permanecer concisos.
- Validação: atualizada a seção 16 do `AGENTS.md` e relido este registro.

## 2026-08-21

### Sprint 16 — Projetos/Listas e agrupamento principal de tarefas

- Adicionado o domínio `Projeto` (`Id`, `Nome`, `NomeNormalizado`), com nome obrigatório de até 100 caracteres, trim, normalização invariável em maiúsculas e índice único. Foram incluídos `GET/POST/DELETE /api/projetos`; o delete não remove tarefas.
- Criada a relação opcional `Projeto 1:N Tarefa` por `TAREFAS.PROJETO_ID`, índice na FK e `ON DELETE SET NULL`. Os contratos de tarefa agora recebem `projetoId`, respondem com `projeto`, filtram por `projetoId` antes de ordenar/paginar e registram `AlteracaoProjeto` com os nomes no histórico. A criação com projeto gera somente o evento de criação.
- Atualizado o frontend com seleção única, criação e gerenciamento contextual de projetos, opção de limpar a associação, filtro persistido na URL, coluna na listagem, detalhes e timeline.
- Validações: baseline 92/92; validação final com build backend sem avisos/erros e 94/94 testes aprovados, lint e build frontend aprovados. Testes SQLite cobrem unicidade, ordenação, `SET NULL` preservando tarefa e combinação de projeto com etiqueta antes da paginação. A migration `20260821164100_AdicionarProjetos` foi aplicada com sucesso em banco vazio e em banco isolado migrado da Sprint 15.
- Limitações: teste HTTP funcional A–N e validação visual não foram executados nesta sessão. Bancos isolados de migration permanecem em `TempSprint16` para limpeza manual.

### Sprint 15 — Etiquetas e organização por classificação

- Adicionado o domínio `Etiqueta`, com nome obrigatório de até 50 caracteres, trim, normalização invariável em maiúsculas e índice único em `NOME_NORMALIZADO`; nomes equivalentes por caixa ou espaços não podem coexistir.
- Criada a relação N:N EF Core entre `TAREFAS` e `ETIQUETAS`, pela tabela `TAREFA_ETIQUETA` de chave composta. As duas FKs removem apenas associações no delete físico; a exclusão lógica/restauração de tarefas preserva etiquetas.
- Integrados DTOs, endpoints `GET/POST/DELETE /api/etiquetas`, associação por `etiquetaIds` na criação/edição de tarefas, retorno de etiquetas, filtro `etiquetaId`, e histórico `AlteracaoEtiquetas` em JSON de nomes ordenados.
- Atualizado o frontend com Autocomplete múltiplo, criação contextual, gerenciador com aviso de exclusão global, chips em listagem/detalhes, timeline e filtro persistido na URL.
- Validações: baseline 90/90; validação posterior backend 0 avisos/0 erros e 92/92 testes aprovados; lint e build do frontend aprovados. Testes SQLite cobriram unicidade normalizada, ordenação, N:N, filtro antes de paginação e limpeza de associações ao excluir etiqueta. Migrations aplicadas com sucesso em banco vazio isolado e em banco isolado na versão Sprint 14.
- Limitações: a tentativa de teste HTTP funcional isolado foi bloqueada pela política do executor antes de iniciar a API ou criar dados. NÃO EXECUTADO VISUALMENTE. `TempSprint15` contém somente bancos temporários de validação e permanece para limpeza manual, conforme a limitação já registrada para diretórios temporários.

### Sprint 12 — Prontidão operacional

- Adicionado `GET /health` com Health Checks nativos e verificação de conectividade do SQLite, sem expor detalhes sensíveis.
- Validado no startup `ConnectionStrings:DefaultConnection` e `Cors:AllowedOrigins`; o diretório configurado do SQLite é criado sem criar/aplicar banco ou migrations automaticamente.
- Corrigida a migration histórica `20260715135748_CorrigirBancoExcluidaEm`: a operação duplicava a criação de `EXCLUIDA_EM`; agora é no-op, preservando a coluna criada pela migration anterior e a compatibilidade de histórico.
- Adicionados scripts PowerShell de backup/restore e a pequena ferramenta administrativa `scripts/DatabaseMaintenance`, que usa a API de backup do SQLite e preserva o banco atual antes de restore.
- Validações: baseline e validação incremental de build sem warnings/erros; testes 70/70 aprovados; cadeia completa de migrations aplicada em banco vazio isolado; backup real criado em banco isolado.
- Pendências: registrar a validação completa de restore, health e limpeza dos bancos temporários ao encerrar a Sprint.

### Sprint 12 — Homologação operacional final

- Executados em área isolada `TempSprint12`: migrations em banco vazio e em cópia do banco existente; ambos concluíram com sucesso pela cadeia real do EF Core.
- Health em runtime: banco SQLite disponível retornou HTTP 200; cenário controlado com `Data Source` apontando para diretório retornou HTTP 503, com a API iniciada normalmente.
- Fail-fast: `DefaultConnection` com `Data Source=` e CORS com `localhost:3000` encerraram o startup antes da escuta, cada qual com mensagem clara de configuração.
- Backup/restore: criada a tarefa isolada `SPRINT12-RESTORE-VALIDATION`, realizado backup pelo script, removida a tarefa após o backup, restaurado o backup e preservado o banco pré-restore. Após restore, a tarefa foi recuperada, `PRAGMA integrity_check` retornou `ok` pela operação administrativa `integrity-check`, e `/health` retornou HTTP 200.
- A ferramenta `scripts/DatabaseMaintenance` recebeu a operação administrativa mínima `integrity-check <banco>` para executar `PRAGMA integrity_check` sem nova dependência, retornando código não-zero quando o resultado não for `ok`.
- Validação final: backend build com 0 avisos e 0 erros; testes 70/70 aprovados, 0 falhas e 0 ignorados; frontend lint e build aprovados; `git diff --check` sem erros. Não executado visualmente — não aplicável como critério de fechamento operacional desta Sprint.
- Processos temporários da API: encerrados; porta 5025 sem listener ao final.
- Limpeza: a remoção de `TempSprint12` foi tentada após conferir seu conteúdo exclusivamente temporário, mas foi bloqueada pela política do executor. Pendente de remoção manual: `C:\xpto\ProjetoTarefas\TempSprint12`.

### Criação do diário

- Criado este arquivo para manter o histórico diário das atividades do projeto.
- Inspecionado o estado inicial do Git: já havia alterações não relacionadas em `README.md`, backend e frontend; elas foram preservadas e não foram modificadas nesta atividade.
- Validação: conferida a existência do arquivo e o estado do repositório antes da alteração.

### Regra permanente de atualização

- Adicionada ao `AGENTS.md` a regra obrigatória de atualizar o `DIARIO.md` ao concluir cada tarefa ou etapa relevante.
- A regra determina o registro da data, das alterações realizadas, das validações e de pendências, sem sobrescrever o histórico anterior.
- Validação: relidos `AGENTS.md` e este diário após a alteração.

### Sprint 11 — Dashboard e refinamento de UX

- Estendido o resumo de tarefas com contadores agregados no SQLite para vencidas, vencem hoje e próximas, usando a data de negócio fornecida pelo `TimeProvider` no service.
- Adicionados cards navegáveis no dashboard para os filtros de prazo existentes, chips textuais de prioridade, uma timeline de histórico e feedback de sucesso contextual para as operações principais.
- Reforçada a confirmação de exclusão permanente com a consequência sobre o histórico, sem adicionar dependências, migrations ou novos domínios.
- Validações executadas: baseline e validação final de `dotnet build ProjetoTarefas.slnx`, `dotnet test ProjetoTarefas.slnx --no-build` (70/70), `npm run lint` e `npm run build`, todos concluídos com sucesso.
- Limitação: não houve navegador disponível para validação visual ou fluxos funcionais interativos; nenhum dado temporário foi criado.

### Sprint 13 — Evolução do ciclo de vida da tarefa e auditoria de transições

- Formalizada no `TarefaService` a matriz explícita entre `Pendente`, `Em andamento` e `Concluída`: toda transição entre estados distintos é permitida; a permanência no mesmo estado não causa efeitos colaterais.
- Centralizada a aplicação da transição: atualização da situação, `SituacaoAlteradaEm`, `ConcluidaEm` e histórico usam o mesmo instante UTC obtido pelo `TimeProvider` e permanecem no único `SaveChangesAsync` da atualização.
- Mantidos os eventos compatíveis `Conclusao` e `Reabertura`, agora com `Campo=Situacao`, `ValorAnterior` e `ValorNovo`; adicionado `AlteracaoSituacao` para `Pendente ↔ Em andamento`. Eventos históricos antigos continuam legíveis na timeline.
- Atualizado o tipo e a timeline do frontend para exibir `Situação alterada` e origem → destino, sem alteração de rota, DTO, schema ou migration.
- Validações: baseline 70/70; após a Sprint, build backend com 0 avisos e 0 erros, 81/81 testes aprovados (0 falhas, 0 ignorados), lint e build frontend aprovados, `git diff --check` sem erros de whitespace.
- Cobertura adicionada para as seis transições, três casos sem mudança, entrada/saída/reentrada em concluída, campos estruturados do histórico, `TimeProvider` e múltiplas alterações em um único save.
- Limitações: não executado teste funcional HTTP contra banco isolado para não modificar o banco normal sem ambiente isolado provisionado. NÃO EXECUTADO VISUALMENTE.

### Sprint 14 — Observações da tarefa

- Adicionado o campo opcional `Observacoes` à entidade, aos contratos de criação, atualização e resposta, com máximo de 4000 caracteres, normalização para `null` quando vazio/somente whitespace e preservação de texto multiline após `Trim` externo.
- Criada a migration `20260821154837_AdicionarObservacoesTarefa`, que adiciona `OBSERVACOES` nullable (`TEXT`, máximo 4000) sem default nem migração de dados. O histórico passou a aceitar valores textuais de até 4000 caracteres e registra `AlteracaoObservacoes` com `Campo=Observacoes` somente quando há alteração normalizada real, no mesmo `SaveChangesAsync` da tarefa.
- Atualizados tipos, schema Zod, criação, edição, detalhes e timeline do frontend. A listagem e a busca permanecem somente pela descrição. Detalhes preservam quebra de linha; a timeline apresenta `De`/`Para`, texto ausente como `Sem observações` e limita visualmente textos longos.
- Validações: baseline 81/81; validação final backend com 0 avisos/0 erros e 90/90 testes aprovados (0 falhas, 0 ignorados); lint e build do frontend aprovados; `git diff --check` sem erros de whitespace. Migration aplicada com sucesso em banco vazio isolado e em cópia isolada do banco existente.
- Limitações: a tentativa de teste HTTP funcional com API isolada foi bloqueada pela política do executor antes de iniciar processo ou criar dados. NÃO EXECUTADO VISUALMENTE. Bancos temporários de validação preservados em `TempSprint14` para limpeza manual, pois a política do executor bloqueia remoções.
