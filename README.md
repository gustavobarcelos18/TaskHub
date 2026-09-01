# TaskHub

Aplicação web para organizar tarefas, projetos e etiquetas. O projeto é formado por um frontend Next.js e uma API ASP.NET Core, que se comunicam por HTTP/JSON e persistem os dados em SQLite.

Esta página apresenta o estado atual do projeto e o fluxo de desenvolvimento. Os detalhes operacionais e o contrato HTTP completo estão nos READMEs do [backend](backend/README.md) e do [frontend](frontend/README.md).

## Visão geral

O TaskHub permite criar, consultar, editar e organizar tarefas, incluindo:

- Situação: `Pendente`, `Em andamento` e `Concluída`.
- Prioridade: `Baixa`, `Media` e `Alta` (`Media` é exibida como “Média” na interface).
- Data de vencimento como data civil, sem conversão de fuso horário.
- Observações opcionais com suporte a múltiplas linhas.
- Associação opcional a um projeto e a múltiplas etiquetas.
- Busca, filtros, ordenação e paginação feitos pela API.
- Histórico auditável das alterações relevantes.
- Exclusão lógica, lixeira, restauração e exclusão permanente controlada.
- Health check para verificação operacional do backend.

## Arquitetura

```text
Usuário
   ↓
Frontend Next.js / React
   ↓ HTTP/JSON
API ASP.NET Core
   ↓
Controller → Service → Repository → AppDbContext → SQLite
```

| Camada | Responsabilidade |
| --- | --- |
| Frontend | Interface, navegação, formulários, feedback e chamadas HTTP. |
| Controller | Contrato HTTP, model binding e códigos de resposta. |
| Service | Regras de negócio, normalização, auditoria e orquestração. |
| Repository | Consultas e persistência com Entity Framework Core. |
| AppDbContext | Mapeamentos, filtros globais e configuração do EF Core. |

O backend não expõe entidades do Entity Framework pela API: requests e responses usam DTOs. A exclusão lógica é aplicada por filtro global, por isso as consultas normais retornam apenas tarefas ativas.

## Tecnologias

### Frontend

- Next.js 16 com App Router, React 19 e TypeScript.
- Material UI e MUI X Data Grid.
- React Hook Form e Zod para formulários e validação.
- `fetch` nativo, centralizado em services por funcionalidade.

### Backend

- .NET 10 e ASP.NET Core.
- Entity Framework Core com SQLite.
- Serilog para logs.
- Swagger/OpenAPI em Development e `ProblemDetails` para erros HTTP.
- xUnit para testes automatizados.

## Estrutura do repositório

```text
ProjetoTarefas/
├── backend/
│   ├── MinhaPrimeiraAPI/        # ProjetoTarefas: API, EF Core, migrations e SQLite
│   └── MinhaPrimeiraAPI.Tests/  # ProjetoTarefas.Tests: testes de services e repositories
├── frontend/
│   └──                           # Next.js, componentes e services HTTP
├── scripts/                       # Backup, restore e manutenção do SQLite
├── Database/                      # Bancos locais, ignorados pelo Git
└── ProjetoTarefas.slnx
```

No frontend, a funcionalidade de tarefas está organizada em `features/tarefas`, separando componentes, schemas Zod, services, tipos e utilitários. As rotas usam o App Router.

## Execução local

### Pré-requisitos

- .NET SDK 10.
- Node.js e npm compatíveis com o `package.json` do frontend.

### 1. Iniciar o backend

Na raiz do repositório:

```powershell
dotnet run --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj
```

A API local usa `https://localhost:7056`. Em Development, o Swagger está disponível em `https://localhost:7056/swagger` e o health check em `https://localhost:7056/health`.

### 2. Configurar e iniciar o frontend

```powershell
Set-Location frontend
Copy-Item .env.example .env.local
npm install
npm run dev
```

Em `.env.local`, mantenha a URL da API:

```env
BACKEND_API_URL=https://localhost:7056
```

O frontend local utiliza HTTPS e encaminha `/api/*` para `BACKEND_API_URL`. Também estão disponíveis `npm run dev:api` e `npm run dev:all` para iniciar a API a partir do diretório do frontend.

## Principais fluxos da interface

| Rota | Função |
| --- | --- |
| `/` | Página inicial com atalhos para os fluxos principais. |
| `/tarefas` | Grade paginada de tarefas, busca, filtros, ordenação e ações. |
| `/tarefas/criar` | Formulário de criação de tarefa. |
| `/tarefas/[id]` | Detalhes da tarefa. |
| `/tarefas/selecionar/[modo]` | Seleção de tarefa para detalhes, edição ou histórico. |
| `/tarefas/lixeira` | Tarefas excluídas logicamente, com restauração ou remoção permanente. |

Os formulários validam descrição, situação, prioridade e demais dados no cliente, mas a API continua responsável por proteger as regras de negócio. A interface usa feedbacks de carregamento, erro e sucesso com componentes Material UI.

## Modelo e regras de negócio

Uma tarefa possui descrição obrigatória de até 200 caracteres, situação, prioridade, data de vencimento, observações, projeto e etiquetas. Campos de auditoria como `criadaEm`, `modificadaEm`, `situacaoAlteradaEm`, `concluidaEm` e `excluidaEm` são instantes UTC gerados pelo backend.

| Regra | Comportamento |
| --- | --- |
| Criação | Situação padrão `Pendente` e prioridade padrão `Media` quando ausentes. |
| Normalização | Textos recebem trim; observações vazias tornam-se `null`. |
| Situação | Transições entre os três estados são permitidas; repetir a mesma situação não gera efeito colateral. |
| Conclusão | Ao entrar em `Concluída`, registra `concluidaEm`; ao sair, o campo é limpo. |
| Projeto | É opcional. Excluir um projeto preserva as tarefas, removendo apenas a associação. |
| Etiquetas | São opcionais e múltiplas. Excluir uma etiqueta remove suas associações, sem excluir tarefas. |
| Exclusão | O `DELETE` comum envia a tarefa à lixeira; a exclusão física só é permitida para tarefa já excluída logicamente. |
| Histórico | Registra criação, descrição, situação, prioridade, vencimento, observações, projeto, etiquetas, exclusão e restauração. |

## API HTTP

As rotas principais são:

| Método | Rota | Descrição |
| --- | --- | --- |
| GET | `/health` | Verifica a disponibilidade da API e do SQLite. |
| GET | `/api/tarefas` | Lista tarefas ativas com filtros, ordenação e paginação. |
| GET | `/api/tarefas/excluidas` | Lista a lixeira. |
| GET | `/api/tarefas/{id}` | Consulta uma tarefa ativa. |
| GET | `/api/tarefas/{id}/historico` | Consulta o histórico, inclusive de tarefa na lixeira. |
| POST | `/api/tarefas` | Cria uma tarefa. |
| PUT | `/api/tarefas/{id}` | Atualiza uma tarefa ativa. |
| DELETE | `/api/tarefas/{id}` | Executa exclusão lógica. |
| PATCH | `/api/tarefas/{id}/restaurar` | Restaura tarefa da lixeira. |
| DELETE | `/api/tarefas/{id}/permanente` | Exclui permanentemente tarefa na lixeira. |
| GET, POST, DELETE | `/api/etiquetas` e `/api/etiquetas/{id}` | Gerencia etiquetas. |
| GET, POST, DELETE | `/api/projetos` e `/api/projetos/{id}` | Gerencia projetos. |

`GET /api/tarefas` aceita `busca`, `situacao`, `prioridade`, `prazo`, `etiquetaId`, `projetoId`, `ordenarPor`, `direcao`, `pagina` e `tamanhoPagina`. Os filtros e a paginação são executados no banco antes de o resultado ser retornado.

Erros controlados seguem o formato `ProblemDetails`. IDs inválidos e consultas inválidas retornam `400`; recursos inexistentes, `404`; conflitos de negócio, como restaurar tarefa ativa ou criar projeto/etiqueta duplicado, retornam `409`.

Para o contrato de requests, responses, valores aceitos e exemplos, consulte o [README do backend](backend/README.md#contrato-http).

## Configuração por ambiente

O backend usa a configuração padrão do ASP.NET Core: `appsettings.json` contém valores compartilhados e `appsettings.{Environment}.json` prevalece sobre ele.

| Ambiente | Arquivo | Uso |
| --- | --- | --- |
| `Development` | `appsettings.Development.json` | SQLite local e host local. |
| `Homologation` | `appsettings.Homologation.json` | Requer conexão e hosts fornecidos pela infraestrutura. |
| `Production` | `appsettings.Production.json` | Requer os valores operacionais fora do repositório. |

Em Homologation e Production, informe ao menos `ConnectionStrings__DefaultConnection` e `AllowedHosts` no ambiente de hospedagem. O frontend usa um rewrite same-origin e recebe o endereço interno do backend por `BACKEND_API_URL`; a API não configura CORS para esse fluxo. Não versione segredos, URLs definitivas ou caminhos de volumes persistentes. O [README do backend](backend/README.md#ambientes) contém um exemplo completo em PowerShell e o procedimento de migrations.

## Banco, migrations e manutenção

Migrations não são aplicadas no startup. Para preparar o banco, execute explicitamente:

```powershell
dotnet ef database update --project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj --startup-project backend/MinhaPrimeiraAPI/ProjetoTarefas.csproj
```

Os scripts `backup-database.ps1` e `restore-database.ps1` utilizam a API de backup do SQLite. Pare a API antes dessas operações e siga as instruções do [README do backend](backend/README.md#backup-e-restore).

## Validação

Backend, a partir da raiz:

```powershell
dotnet build ProjetoTarefas.slnx
dotnet test ProjetoTarefas.slnx --no-build
```

Frontend:

```powershell
Set-Location frontend
npm run lint
npm run build
```

Os testes backend cobrem regras de service e consultas de repository com SQLite real em memória, incluindo filtros globais, ordenação, paginação, histórico, projetos e etiquetas. O total de testes é evolutivo; consulte a saída do comando para o resultado da versão em execução.
