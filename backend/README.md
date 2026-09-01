# ProjetoTarefas

## Visão geral

API HTTP para tarefas. Usa ASP.NET Core, Entity Framework Core, SQLite, Serilog, Swagger/OpenAPI e `ProblemDetails`. A API mantém exclusão lógica, lixeira, histórico, filtros, ordenação e paginação.

## Pré-requisitos e execução

- .NET SDK 10.
- Para migrations, a ferramenta `dotnet-ef` compatível com o SDK.

O perfil local de desenvolvimento usa exclusivamente `https://localhost:7056`.

```powershell
dotnet run --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj --launch-profile https
```

Em Development, o Swagger UI está em `https://localhost:7056/swagger` e o documento OpenAPI em `/swagger/v1/swagger.json`. Swagger não é habilitado fora de Development.

## Operação

### Health check

`GET /health` usa o mecanismo nativo de Health Checks do ASP.NET Core e confirma que a API responde e que o SQLite aceita conexão. Retorna `200` quando saudável e `503` quando o banco não está acessível. A resposta padrão não expõe connection string, caminhos, exceções ou outros detalhes internos.

### Configuração

No startup, `ConnectionStrings:DefaultConnection` precisa estar preenchida e informar `Data Source`. Falhas encerram o startup com uma mensagem clara no log. O diretório do SQLite é criado quando necessário; o banco em si continua sendo criado somente por migrations.

O frontend consome a API pelo rewrite same-origin `/api` do Next.js; por isso a API não configura CORS para o navegador da aplicação.

### Ambientes

O host do ASP.NET Core carrega `appsettings.json` e, em seguida, o arquivo `appsettings.{ASPNETCORE_ENVIRONMENT}.json`; o segundo prevalece sobre o primeiro. O repositório possui os seguintes ambientes:

| Ambiente | Arquivo | Uso e comportamento |
| --- | --- | --- |
| `Development` | `appsettings.Development.json` | Uso local. Configura SQLite em `Database/tarefas.db` e host `localhost`. |
| `Homologation` | `appsettings.Homologation.json` | Validação prévia à produção. Exige que conexão e hosts sejam fornecidos pela infraestrutura. |
| `Production` | `appsettings.Production.json` | Ambiente produtivo. Também exige conexão e hosts fora do repositório. |

Homologação e Produção deixam `DefaultConnection` e `AllowedHosts` propositalmente vazios. O fail-fast da conexão impede o uso de um banco local por engano. Não inclua URLs definitivas, caminhos de volume, credenciais ou segredos nesses arquivos versionados.

Exemplo de inicialização em Homologação no PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Homologation"
$env:ConnectionStrings__DefaultConnection = "Data Source=C:\dados\projetotarefas-hml\tarefas.db"
$env:AllowedHosts = "hml.api.exemplo.com"
dotnet run --no-launch-profile --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj
```

O `--no-launch-profile` é necessário nesse exemplo porque `Properties/launchSettings.json` é destinado à execução local e define `Development`. Para Produção, use `ASPNETCORE_ENVIRONMENT=Production` e valores equivalentes apontando para o volume persistente e os domínios produtivos. Em servidores, cadastre essas variáveis no mecanismo de configuração da plataforma, e não em scripts ou arquivos versionados.

### Migrations e ambiente novo

A API não executa migrations no startup. Aplique-as explicitamente antes de iniciar a API:

```powershell
dotnet ef database update --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj --startup-project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj
```

Para um banco novo em outro local, informe uma connection string isolada:

```powershell
dotnet ef database update --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj --startup-project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj --connection "Data Source=C:\dados\tarefas.db"
```

A sequência manual de implantação é: configurar as variáveis de ambiente, aplicar migrations, iniciar o backend, verificar `/health`, configurar `BACKEND_API_URL` no frontend e iniciar/buildar o frontend.

### Backup e restore

Os scripts PowerShell usam a API de backup do SQLite, em vez de copiar o arquivo enquanto ele pode estar em escrita. Eles verificam a integridade da origem antes da operação. Compile o backend uma vez para disponibilizar o provedor SQLite e pare a API antes da operação; o parâmetro explícito `-ApiStopped` evita execução acidental com a API ativa.

```powershell
dotnet build ProjetoTarefas.slnx
.\scripts\backup-database.ps1 -ApiStopped
.\scripts\restore-database.ps1 -BackupPath .\backups\tarefas-AAAAMMDD-HHMMSS-fff.db -ApiStopped
```

O backup é salvo por padrão em `backups/tarefas-AAAAMMDD-HHMMSS-fff.db`, diretório ignorado pelo Git. O restore preserva antes o banco atual como `tarefas-pre-restore-AAAAMMDD-HHMMSS-fff.db` no diretório do banco. Depois do restore, inicie a API, consulte `/health` e confirme os dados esperados. Os scripts são administrativos, sem endpoint HTTP de backup.

### Logging e limitações

Serilog grava no console e em `Logs/api-.log`, com arquivo diário, rotação por 10 MB e retenção de 30 arquivos. O nível padrão é Information, reduzindo ASP.NET Core a Warning. Não há agendamento, criptografia ou envio externo de backups nesta aplicação.

### Dashboard técnico de logs

Os eventos estruturados também são persistidos em `Database/logs.db`, separado do banco de negócio `tarefas.db`. A tabela própria mantém os campos técnicos conhecidos (nível, evento, usuário, método, caminho, status, duração e `TraceIdentifier`) e uma allowlist de propriedades seguras. A retenção padrão é de 30 dias e é aplicada na inicialização, sem limpeza por requisição.

`GET /api/logs` exige autenticação e `TechnicalDiagnostics:Enabled`. A consulta tem paginação server-side, ordenação mais recente primeiro e filtros por nível, usuário, período, status, método, caminho, trace ID e texto. Consultas do próprio dashboard são omitidas da visão padrão para evitar que a tela seja dominada por seus próprios acessos, mas continuam armazenadas.

## Configuração e banco

A chave `ConnectionStrings:DefaultConnection` de `appsettings.Development.json` aponta, por padrão, para o SQLite `Database/tarefas.db` relativo ao projeto backend. Configurações ASP.NET Core podem ser sobrescritas por variáveis de ambiente, como `ConnectionStrings__DefaultConnection`. Não inclua segredos em arquivos versionados.

O navegador acessa somente o Next.js, que encaminha `/api/*` para `BACKEND_API_URL`. Se outro cliente web passar a chamar a API diretamente de uma origem diferente, será necessário projetar e configurar uma política CORS explícita para esse novo cenário.

O aplicativo não aplica migrations automaticamente. Para aplicar a cadeia existente:

```powershell
dotnet ef database update --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj --startup-project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj
```

Para criar uma migration futura:

```powershell
dotnet ef migrations add NomeDaMigration --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj --startup-project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj
```

Arquivos `.db`, `.db-shm` e `.db-wal` em `Database/` são ignorados pelo Git. A migration `20260715135748_CorrigirBancoExcluidaEm` é intencionalmente sem alteração de schema: ela substitui a operação duplicada que tentava criar `EXCLUIDA_EM` pela segunda vez. Bancos existentes que já a registraram continuam compatíveis; bancos novos passam a aplicar a cadeia completa.

## Contrato HTTP

| Método | Rota | Sucesso | Erros documentados |
| --- | --- | --- | --- |
| GET | `/api/tarefas` | 200 `TarefasPaginadasResponse` | 400 |
| GET | `/api/tarefas/excluidas` | 200 `TarefaResponse[]` | — |
| GET | `/api/tarefas/{id}` | 200 `TarefaResponse` | 400, 404 |
| GET | `/api/tarefas/{id}/historico` | 200 `HistoricoTarefaResponse[]` | 400, 404 |
| POST | `/api/tarefas` | 201 `TarefaResponse` | 400 |
| PUT | `/api/tarefas/{id}` | 204 | 400, 404 |
| DELETE | `/api/tarefas/{id}` | 204 | 400, 404 |
| PATCH | `/api/tarefas/{id}/restaurar` | 204 | 400, 404, 409 |
| DELETE | `/api/tarefas/{id}/permanente` | 204 | 400, 404, 409 |
| GET | `/api/etiquetas` | 200 `EtiquetaResponse[]` | — |
| POST | `/api/etiquetas` | 201 `EtiquetaResponse` | 400, 409 |
| DELETE | `/api/etiquetas/{id}` | 204 | 400, 404 |
| GET | `/api/projetos` | 200 `ProjetoResponse[]` | — |
| POST | `/api/projetos` | 201 `ProjetoResponse` | 400, 409 |
| DELETE | `/api/projetos/{id}` | 204 | 400, 404 |

`id` deve ser maior que zero. O detalhe normal só encontra tarefas ativas; o histórico continua disponível para uma tarefa excluída logicamente. A lixeira lista exclusões lógicas. A exclusão permanente requer que a tarefa já esteja na lixeira e também remove seu histórico.

### Criar e atualizar

`POST /api/tarefas` aceita `descricao` obrigatória (até 200 caracteres), `situacao` opcional, `prioridade` opcional, `dataVencimento` opcional e `projetoId` opcional. Os padrões de criação são `Pendente` e `Media`. `PUT /api/tarefas/{id}` exige `descricao`, `situacao` e `prioridade`; `dataVencimento` e `projetoId` continuam opcionais e podem ser `null`. A resposta inclui o objeto `projeto` quando houver associação.

- Situação: `Pendente`, `Em andamento`, `Concluída`.
- Prioridade: `Baixa`, `Media`, `Alta` (`Media` é o valor da API; a interface apresenta “Média”).
- `dataVencimento`: data civil `yyyy-MM-dd`, sem horário ou timezone.

```http
POST /api/tarefas
Content-Type: application/json

{ "descricao": "Preparar relatório", "situacao": "Em andamento", "prioridade": "Alta", "dataVencimento": "2030-01-10" }
```

### Listagem, filtros, ordenação e paginação

`GET /api/tarefas` aceita estes query parameters opcionais:

| Parâmetro | Tipo/valores | Padrão e regra |
| --- | --- | --- |
| `busca` | texto | Busca por descrição; espaços externos são removidos. |
| `situacao` | valores de situação | Comparação canônica; inválido retorna 400. |
| `prioridade` | `Baixa`, `Media`, `Alta` | Aceita a normalização definida pela API; inválido retorna 400. |
| `prazo` | `vencidas`, `vencemHoje`, `proximas`, `semVencimento` | Sem filtro quando ausente. |
| `projetoId` | inteiro positivo | Filtra tarefas pelo projeto antes da ordenação e paginação. |
| `ordenarPor` | `descricao`, `situacao`, `prioridade`, `dataVencimento`, `ultimaAtualizacao` | `ultimaAtualizacao`. |
| `direcao` | `asc`, `desc` | `desc`. |
| `pagina` | inteiro ≥ 1 | `1`; inválido retorna 400. |
| `tamanhoPagina` | inteiro de 1 a 100 | `10`; inválido retorna 400. |

`vencidas` significa vencimento anterior à data de negócio e tarefa não concluída; `vencemHoje` exige igualdade; `proximas` exige data posterior. `semVencimento` seleciona `dataVencimento = null`.

A resposta paginada possui `itens`, `paginaAtual`, `tamanhoPagina`, `totalItens` e `totalPaginas`. Uma página sem resultados retorna lista vazia e preserva os metadados calculados.

```http
GET /api/tarefas?prioridade=Alta&prazo=vencidas&ordenarPor=dataVencimento&direcao=asc&pagina=1&tamanhoPagina=10
```

### Histórico e datas

O histórico retorna `id`, `tipo`, `campo`, `valorAnterior`, `valorNovo` e `criadoEm`, em ordem decrescente de criação. Tipos de histórico: `Criacao`, `AlteracaoDescricao`, `AlteracaoPrioridade`, `AlteracaoDataVencimento`, `Conclusao`, `Reabertura`, `Exclusao` e `Restauracao`.

Campos de auditoria (`criadaEm`, `modificadaEm`, `situacaoAlteradaEm`, `concluidaEm`, `excluidaEm` e `criadoEm` do histórico) representam instantes UTC gerados pelo backend. Eles são distintos de `dataVencimento`.

## Erros

Erros controlados usam `ProblemDetails` com `type`, `title`, `status`, `detail`, `instance` e `traceId`. Erros de validação automática do `[ApiController]` usam `ValidationProblemDetails`, que acrescenta `errors`. Os status 400, 404 e 409 usam esse formato. Exceções não tratadas retornam 500 com `ProblemDetails` e `traceId`.

## Testes

Na raiz do repositório:

```powershell
dotnet build ProjetoTarefas.slnx
dotnet test ProjetoTarefas.slnx
```

Os testes atuais incluem regras de `TarefaService`, normalização de consultas e testes de `TarefaRepository` com SQLite real em memória. Estes últimos cobrem tradução de consultas, filtros globais, `DateOnly`, ordenação, paginação, histórico e cascade delete.
