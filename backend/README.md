# MinhaPrimeiraAPI

## Visão geral

API HTTP para tarefas. Usa ASP.NET Core, Entity Framework Core, SQLite, Serilog, Swagger/OpenAPI e `ProblemDetails`. A API mantém exclusão lógica, lixeira, histórico, filtros, ordenação e paginação.

## Pré-requisitos e execução

- .NET SDK 10.
- Para migrations, a ferramenta `dotnet-ef` compatível com o SDK.

O perfil HTTP de desenvolvimento usa `http://localhost:5025`; o perfil HTTPS também oferece `https://localhost:7056`.

```powershell
dotnet run --project backend/MinhaPrimeiraAPI
```

Em Development, o Swagger UI está em `http://localhost:5025/swagger` e o documento OpenAPI em `/swagger/v1/swagger.json`. Swagger não é habilitado fora de Development.

## Operação

### Health check

`GET /health` usa o mecanismo nativo de Health Checks do ASP.NET Core e confirma que a API responde e que o SQLite aceita conexão. Retorna `200` quando saudável e `503` quando o banco não está acessível. A resposta padrão não expõe connection string, caminhos, exceções ou outros detalhes internos.

### Configuração e CORS

No startup, `ConnectionStrings:DefaultConnection` precisa estar preenchida e informar `Data Source`; `Cors:AllowedOrigins` precisa conter ao menos uma URL HTTP/HTTPS absoluta. Falhas encerram o startup com uma mensagem clara no log. O diretório do SQLite é criado quando necessário; o banco em si continua sendo criado somente por migrations.

O padrão de CORS é `http://localhost:3000`, sem `AllowAnyOrigin`. Para outro ambiente, defina por exemplo `Cors__AllowedOrigins__0=https://app.exemplo.com` e mantenha uma lista explícita de origens.

### Migrations e ambiente novo

A API não executa migrations no startup. Aplique-as explicitamente antes de iniciar a API:

```powershell
dotnet ef database update --project backend/MinhaPrimeiraAPI --startup-project backend/MinhaPrimeiraAPI
```

Para um banco novo em outro local, informe uma connection string isolada:

```powershell
dotnet ef database update --project backend/MinhaPrimeiraAPI --startup-project backend/MinhaPrimeiraAPI --connection "Data Source=C:\dados\tarefas.db"
```

A sequência manual de implantação é: configurar as variáveis de ambiente, aplicar migrations, iniciar o backend, verificar `/health`, configurar `NEXT_PUBLIC_API_URL` no frontend e iniciar/buildar o frontend.

### Backup e restore

Os scripts PowerShell usam a API de backup do SQLite, em vez de copiar o arquivo enquanto ele pode estar em escrita. Compile o backend uma vez para disponibilizar o provedor SQLite e pare a API antes da operação; o parâmetro explícito `-ApiStopped` evita execução acidental com a API ativa.

```powershell
dotnet build ProjetoTarefas.slnx
.\scripts\backup-database.ps1 -ApiStopped
.\scripts\restore-database.ps1 -BackupPath .\backups\tarefas-AAAAMMDD-HHMMSS.db -ApiStopped
```

O backup é salvo por padrão em `backups/tarefas-AAAAMMDD-HHMMSS.db`, diretório ignorado pelo Git. O restore preserva antes o banco atual como `tarefas-pre-restore-AAAAMMDD-HHMMSS.db` no diretório do banco. Depois do restore, inicie a API, consulte `/health` e confirme os dados esperados e `PRAGMA integrity_check` em um cliente SQLite. Os scripts são administrativos, sem endpoint HTTP de backup.

### Logging e limitações

Serilog grava no console e em `Logs/api-.log`, com arquivo diário, rotação por 10 MB e retenção de 30 arquivos. O nível padrão é Information, reduzindo ASP.NET Core a Warning. Não há agendamento, criptografia ou envio externo de backups nesta aplicação.

## Configuração, CORS e banco

A chave `ConnectionStrings:DefaultConnection` de `appsettings.json` aponta, por padrão, para o SQLite `Database/tarefas.db` relativo ao projeto backend. Configurações ASP.NET Core podem ser sobrescritas por variáveis de ambiente, como `ConnectionStrings__DefaultConnection`. Não inclua segredos em arquivos versionados.

`Cors:AllowedOrigins` define as origens permitidas; o padrão é `http://localhost:3000`, compatível com o frontend Next.js. Altere essa configuração por ambiente se a origem do frontend mudar. A política não usa `AllowAnyOrigin`.

O aplicativo não aplica migrations automaticamente. Para aplicar a cadeia existente:

```powershell
dotnet ef database update --project backend/MinhaPrimeiraAPI --startup-project backend/MinhaPrimeiraAPI
```

Para criar uma migration futura:

```powershell
dotnet ef migrations add NomeDaMigration --project backend/MinhaPrimeiraAPI --startup-project backend/MinhaPrimeiraAPI
```

Arquivos `.db`, `.db-shm` e `.db-wal` em `Database/` são ignorados pelo Git. A migration `20260715135748_CorrigirBancoExcluidaEm` é intencionalmente sem alteração de schema: ela substitui a operação duplicada que tentava criar `EXCLUIDA_EM` pela segunda vez. Bancos existentes que já a registraram continuam compatíveis; bancos novos passam a aplicar a cadeia completa.

## Contrato HTTP

| Método | Rota | Sucesso | Erros documentados |
| --- | --- | --- | --- |
| GET | `/api/tarefas` | 200 `TarefasPaginadasResponse` | 400 |
| GET | `/api/tarefas/resumo` | 200 `ResumoTarefasResponse` | — |
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

`id` deve ser maior que zero. O detalhe normal só encontra tarefas ativas; o histórico continua disponível para uma tarefa excluída logicamente. A lixeira lista exclusões lógicas. A exclusão permanente requer que a tarefa já esteja na lixeira e também remove seu histórico.

### Criar e atualizar

`POST /api/tarefas` aceita `descricao` obrigatória (até 200 caracteres), `situacao` opcional, `prioridade` opcional e `dataVencimento` opcional. Os padrões de criação são `Pendente` e `Media`. `PUT /api/tarefas/{id}` exige `descricao`, `situacao` e `prioridade`; `dataVencimento` continua opcional e pode ser `null`.

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
| `ordenarPor` | `descricao`, `situacao`, `prioridade`, `dataVencimento`, `ultimaAtualizacao` | `ultimaAtualizacao`. |
| `direcao` | `asc`, `desc` | `desc`. |
| `pagina` | inteiro ≥ 1 | `1`; inválido retorna 400. |
| `tamanhoPagina` | inteiro de 1 a 100 | `10`; inválido retorna 400. |

`vencidas` significa vencimento anterior à data de negócio e tarefa não concluída; `vencemHoje` exige igualdade; `proximas` exige data posterior. `semVencimento` seleciona `dataVencimento = null`.

A resposta paginada possui `itens`, `paginaAtual`, `tamanhoPagina`, `totalItens` e `totalPaginas`. Uma página sem resultados retorna lista vazia e preserva os metadados calculados.

```http
GET /api/tarefas?prioridade=Alta&prazo=vencidas&ordenarPor=dataVencimento&direcao=asc&pagina=1&tamanhoPagina=10
```

### Resumo, histórico e datas

`GET /api/tarefas/resumo` retorna `total`, `pendentes`, `emAndamento` e `concluidas` para tarefas ativas. O histórico retorna `id`, `tipo`, `campo`, `valorAnterior`, `valorNovo` e `criadoEm`, em ordem decrescente de criação. Tipos de histórico: `Criacao`, `AlteracaoDescricao`, `AlteracaoPrioridade`, `AlteracaoDataVencimento`, `Conclusao`, `Reabertura`, `Exclusao` e `Restauracao`.

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
