# Documentação Funcional e Técnica — ProjetoTarefas

## 1. Visão Geral

* [ ] 

A arquitetura está dividida em duas aplicações independentes:

```text
ProjetoTarefas
├── frontend
│   └── Next.js + React + TypeScript + Material UI
│
└── backend
    └── ASP.NET Core + Entity Framework Core + SQLite
```

O fluxo geral da aplicação é:

```text
Usuário
   ↓
Frontend Next.js
   ↓ HTTP/JSON
API ASP.NET Core
   ↓
Controller
   ↓
Service
   ↓
Repository
   ↓
Entity Framework Core
   ↓
SQLite
```

---

# 2. FrontEnd

## 2.1 Funcionamento

O frontend é responsável pela interface apresentada ao usuário e pela comunicação com a API.

A aplicação utiliza:

```text
Next.js
React
TypeScript
Material UI
React Hook Form
Zod
fetch nativo
```

A URL da API é obtida por variável de ambiente:

```env
NEXT_PUBLIC_API_URL=http://localhost:5025
```

A comunicação HTTP está centralizada no service de tarefas, evitando chamadas à API espalhadas pelos componentes.

### Rotas principais

```text
/
→ página inicial

/tarefas
→ listagem das tarefas

/tarefas/criar
→ criação de nova tarefa
```

### Estrutura principal

```text
src/
├── app/
│   ├── page.tsx
│   └── tarefas/
│       ├── page.tsx
│       ├── criar/
│       │   └── page.tsx
│       ├── error.tsx
│       └── loading.tsx
│
├── components/
│   └── ComponentesRoteador.tsx
│
├── features/
│   └── tarefas/
│       ├── components/
│       ├── schemas/
│       ├── services/
│       ├── types/
│       └── utils/
│
└── theme/
    ├── theme.ts
    └── ThemeProvider.tsx
```

### Listagem

A tela de tarefas consulta:

```http
GET /api/tarefas
```

e apresenta os registros em uma tabela Material UI.

São exibidos:

```text
Descrição
Situação
Última atualização
Ações
```

A última atualização é calculada por:

```ts
tarefa.modificadaEm ?? tarefa.criadaEm
```

Portanto:

```text
se já foi modificada
→ mostra ModificadaEm

caso contrário
→ mostra CriadaEm
```

### Criação

O formulário permite informar:

```text
Descrição
Situação
```

Após validação, o frontend executa:

```http
POST /api/tarefas
```

A criação utiliza React Hook Form e Zod.

### Edição

A edição é realizada em um Dialog Material UI.

Fluxo:

```text
Menu de ações
→ Editar
→ abre Dialog
→ usuário altera dados
→ frontend valida
→ PUT /api/tarefas/{id}
→ fecha Dialog
→ atualiza listagem
```

### Exclusão

A exclusão disponível no frontend é lógica.

Fluxo:

```text
Menu
→ Excluir
→ Dialog de confirmação
→ DELETE /api/tarefas/{id}
→ backend marca ExcluidaEm
→ registro desaparece da listagem
```

A exclusão não ocorre imediatamente ao clicar na opção.

Existe uma confirmação explícita antes da requisição DELETE.

### Feedback

A interface possui mecanismos de feedback utilizando Material UI, incluindo:

```text
Snackbar
Alert
Skeleton
CircularProgress
Dialog
```

A aplicação pode informar:

```text
operação concluída
erro de comunicação
validação inválida
processamento em andamento
```

---

# 2.2 Regra

As regras do frontend são voltadas principalmente para experiência do usuário, apresentação e prevenção de requisições inválidas.

### Descrição

A descrição:

```text
é obrigatória
não pode ser composta apenas por espaços
possui limite máximo de 200 caracteres
```

Espaços externos são desconsiderados pela validação.

Exemplo:

```text
"   Comprar ração   "

é tratado como:

"Comprar ração"
```

### Situação

As situações utilizadas pela interface são:

```text
Pendente
Em andamento
Concluída
```

Não são apresentados valores arbitrários no seletor.

### Indicador visual

Cada situação possui representação visual por `Chip` Material UI.

```text
Pendente
→ warning

Em andamento
→ info/primary

Concluída
→ success
```

A cor é apenas representação visual.

A regra real continua baseada no valor textual da situação.

### Exclusão

A exclusão deve:

```text
1. exigir confirmação
2. impedir solicitações duplicadas durante processamento
3. chamar a API
4. atualizar a listagem após sucesso
```

O botão passa para estado semelhante a:

```text
Excluindo...
```

enquanto a requisição estiver em processamento.

### Atualização

Uma edição só é enviada depois que os dados forem validados pelo formulário.

Após sucesso:

```text
Dialog fecha
→ router.refresh()
→ listagem é consultada novamente
```

### Datas

Datas recebidas da API são apresentadas em formato brasileiro e ajustadas para o fuso utilizado pela aplicação.

Formato aproximado:

```text
dd/MM/yyyy HH:mm
```

A interface não altera os valores de auditoria.

Ela apenas os apresenta.

---

# 2.3 Validação

O frontend utiliza:

```text
React Hook Form
+
Zod
```

O Zod define o schema dos dados aceitos pelo formulário.

### Descrição

São verificadas condições como:

```text
valor obrigatório
trim
tamanho mínimo
máximo de 200 caracteres
```

O campo também utiliza o limite HTML correspondente para melhorar a experiência de digitação.

### Situação

O valor deve pertencer ao conjunto permitido:

```text
Pendente
Em andamento
Concluída
```

### Exibição de erros

Erros de formulário são apresentados próximos ao respectivo campo.

Erros gerais podem ser apresentados por:

```text
Alert
Snackbar
```

### Erro da API

O frontend diferencia:

```text
resposta HTTP de erro
```

de:

```text
falha de conexão com a API
```

Exemplo:

```text
API retorna 404
→ requisição foi realizada, mas o recurso não existe

Failed to fetch
→ frontend não conseguiu se comunicar com a API
```

### Página de erro

A rota de tarefas possui:

```text
error.tsx
```

que apresenta uma interface amigável quando a consulta principal falha.

Existe uma ação:

```text
Tentar novamente
```

que executa:

```ts
reset()
```

---

# 2.4 Comportamento

### Ao abrir `/tarefas`

```text
Frontend consulta API
→ API retorna tarefas ativas
→ tabela é renderizada
```

Enquanto a rota estiver sendo carregada, `loading.tsx` apresenta Skeletons.

### Quando não existem tarefas

A interface apresenta um estado vazio em vez de uma tabela sem registros.

O usuário recebe uma ação para criar sua primeira tarefa.

### Ao criar

```text
Usuário preenche formulário
→ Zod valida
→ frontend envia POST
→ backend cria
→ frontend redireciona ou atualiza listagem
```

### Ao editar

```text
Usuário abre menu
→ Editar
→ Dialog abre preenchido
→ usuário modifica
→ frontend valida
→ PUT
→ Dialog fecha
→ listagem atualizada
```

### Ao excluir

```text
Usuário abre menu
→ Excluir
→ Dialog pede confirmação
→ usuário confirma
→ DELETE
→ botão fica desabilitado
→ tarefa deixa de aparecer
```

### Se ocorrer erro

```text
requisição falha
→ Dialog permanece aberto quando necessário
→ mensagem de erro é apresentada
→ usuário pode tentar novamente
```

### Responsividade

A interface utiliza os mecanismos de layout do Material UI:

```text
Container
Box
Stack
Paper
TableContainer
breakpoints
```

A tabela pode utilizar rolagem horizontal quando a largura da tela não comportar todas as colunas.

---

# 3. BackEnd

## 3.1 Funcionamento

O backend é uma API REST construída com:

```text
ASP.NET Core
.NET 10
Entity Framework Core
SQLite
Serilog
Swagger/OpenAPI
```

A arquitetura principal é:

```text
Controller
   ↓
Service
   ↓
Repository
   ↓
AppDbContext
   ↓
SQLite
```

### Controller

O `TarefasController` recebe as requisições HTTP.

Rota base:

```http
/api/tarefas
```

Endpoints existentes:

```http
GET /api/tarefas

GET /api/tarefas/{id}

POST /api/tarefas

PUT /api/tarefas/{id}

DELETE /api/tarefas/{id}

DELETE /api/tarefas/{id}/permanente
```

### Service

`TarefaService` concentra regras de negócio.

Ele é responsável por:

```text
criação
normalização
datas de auditoria
atualização
conclusão
reabertura
exclusão lógica
exclusão permanente
mapeamento para DTO
```

### Repository

`TarefaRepository` concentra o acesso aos dados.

O contrato `ITarefaRepository` disponibiliza:

```csharp
ListarAtivasAsync()

BuscarAtivaPorIdAsync()

BuscarIncluindoExcluidasPorIdAsync()

Adicionar()

Remover()

SalvarAlteracoesAsync()
```

### Entity Framework

O `AppDbContext` mapeia a entidade `Tarefa` para:

```text
TAREFAS
```

Principais colunas:

```text
ID
DESCRICAO
SITUACAO
CRIADA_EM
MODIFICADA_EM
SITUACAO_ALTERADA_EM
CONCLUIDA_EM
EXCLUIDA_EM
```

---

# 3.2 Regra

## Criação

Ao criar uma tarefa:

```text
CriadaEm
→ DateTime.UtcNow

SituacaoAlteradaEm
→ mesma data da criação

ModificadaEm
→ null

ExcluidaEm
→ null
```

Caso nenhuma situação seja informada:

```text
Situação = Pendente
```

O Service também executa `Trim()` na descrição.

### Tarefa criada como concluída

Caso a situação seja:

```text
Concluída
```

então:

```text
ConcluidaEm = data da criação
```

Caso contrário:

```text
ConcluidaEm = null
```

---

## Atualização

O Service compara:

```text
descrição atual × nova descrição
situação atual × nova situação
```

### Nenhuma alteração

Quando nenhum valor realmente mudou:

```text
nenhum SaveChanges é executado
ModificadaEm não é alterada
SituacaoAlteradaEm não é alterada
```

### Alteração somente da descrição

```text
Descricao
→ alterada

ModificadaEm
→ atualizada

SituacaoAlteradaEm
→ preservada

ConcluidaEm
→ preservada
```

### Alteração da situação

```text
Situacao
→ atualizada

SituacaoAlteradaEm
→ DateTime.UtcNow

ModificadaEm
→ DateTime.UtcNow
```

### Conclusão

Transição:

```text
Pendente/Em andamento
→ Concluída
```

gera:

```text
ConcluidaEm = DateTime.UtcNow
```

### Reabertura

Transição:

```text
Concluída
→ Pendente
```

ou:

```text
Concluída
→ Em andamento
```

gera:

```text
ConcluidaEm = null
```

---

## Exclusão lógica

A exclusão padrão não remove fisicamente o registro.

O Service executa:

```csharp
tarefa.ExcluidaEm = DateTime.UtcNow;
```

e salva a alteração.

O registro continua armazenado no SQLite.

O `AppDbContext` utiliza:

```csharp
entity.HasQueryFilter(
    tarefa => tarefa.ExcluidaEm == null
);
```

Portanto, consultas normais não retornam tarefas excluídas logicamente.

---

## Exclusão permanente

A remoção física só pode ocorrer se a tarefa já estiver excluída logicamente.

Fluxo:

```text
buscar incluindo excluídas
        ↓
registro existe?
 ├─ não → NãoEncontrada
 └─ sim
       ↓
ExcluidaEm está preenchida?
 ├─ não → TarefaAtiva
 └─ sim → remover fisicamente
```

Resultados possíveis:

```text
Sucesso
NaoEncontrada
TarefaAtiva
```

Uma tarefa ativa não pode ser removida permanentemente.

---

# 3.3 Validação

## Validação de ID

O Controller executa:

```csharp
id <= 0
```

como condição inválida.

Mensagem correspondente:

```text
O ID da tarefa deve ser maior que zero.
```

Isso é aplicado em operações como:

```text
buscar
atualizar
excluir logicamente
excluir permanentemente
```

---

## Validação de persistência

O Entity Framework estabelece:

### Descrição

```text
obrigatória
máximo de 200 caracteres
```

Mapeamento:

```csharp
.HasMaxLength(200)
.IsRequired()
```

### Situação

```text
obrigatória
máximo de 30 caracteres
```

Mapeamento:

```csharp
.HasMaxLength(30)
.IsRequired()
```

### Datas obrigatórias

```text
CriadaEm
SituacaoAlteradaEm
```

### Datas opcionais

```text
ModificadaEm
ConcluidaEm
ExcluidaEm
```

---

## Recurso inexistente

Quando uma tarefa não é encontrada:

```http
404 Not Found
```

Exemplo:

```text
Nenhuma tarefa encontrada com o ID 100.
```

---

## Exclusão permanente inválida

Quando uma tarefa ativa é enviada para exclusão permanente:

```http
409 Conflict
```

O backend impede a remoção.

---

# 3.4 Comportamento

## GET `/api/tarefas`

Fluxo:

```text
Controller
→ Service.ListarAsync
→ Repository.ListarAtivasAsync
→ Entity Framework
→ filtro ExcluidaEm == null
→ banco
```

Resposta:

```http
200 OK
```

com lista de tarefas ativas.

---

## GET `/api/tarefas/{id}`

### ID inválido

```http
GET /api/tarefas/0
```

Resposta:

```http
400 Bad Request
```

### Não encontrada

```http
404 Not Found
```

### Encontrada

```http
200 OK
```

com `TarefaResponse`.

---

## POST `/api/tarefas`

Fluxo:

```text
Request
→ Controller
→ Service
→ cria entidade
→ Repository.Adicionar
→ SaveChanges
→ Response
```

Resposta:

```http
201 Created
```

O retorno utiliza `CreatedAtAction`.

---

## PUT `/api/tarefas/{id}`

Se a tarefa existir:

```http
204 No Content
```

Se não existir:

```http
404 Not Found
```

Se o ID for inválido:

```http
400 Bad Request
```

---

## DELETE `/api/tarefas/{id}`

Executa exclusão lógica.

### Sucesso

```http
204 No Content
```

### Não encontrada

```http
404 Not Found
```

### ID inválido

```http
400 Bad Request
```

Após a operação, o registro permanece fisicamente no banco com:

```text
EXCLUIDA_EM != NULL
```

---

## DELETE `/api/tarefas/{id}/permanente`

### Tarefa excluída logicamente

```http
204 No Content
```

### Tarefa inexistente

```http
404 Not Found
```

### Tarefa ainda ativa

```http
409 Conflict
```

### ID inválido

```http
400 Bad Request
```

---

# 4. DTOs e Comunicação

O backend não expõe diretamente a entidade `Tarefa`.

São utilizados DTOs para comunicação.

Fluxo de entrada:

```text
JSON
→ Request DTO
→ Service
→ Entity
```

Fluxo de saída:

```text
Entity
→ TarefaResponse
→ JSON
```

O `TarefaResponse` contempla:

```text
Id
Descricao
Situacao
CriadaEm
ModificadaEm
SituacaoAlteradaEm
ConcluidaEm
ExcluidaEm
```

---

# 5. Logs

O backend utiliza Serilog e `ILogger`.

São registrados eventos como:

```text
inicialização
listagem
criação
atualização
exclusão
recurso não encontrado
operações rejeitadas
erros inesperados
```

Exemplo conceitual:

```text
Tarefa atualizada.
TarefaId=10
DescricaoAlterada=True
SituacaoAlterada=False
```

Os logs utilizam propriedades estruturadas em vez de simples concatenação de texto.

---

# 6. Testes Automatizados

Existe um projeto separado:

```text
MinhaPrimeiraAPI.Tests
```

utilizando:

```text
xUnit
```

Os testes unitários verificam regras do `TarefaService`.

Atualmente foram executados:

```text
Total: 20
Sucesso: 20
Falhas: 0
Ignorados: 0
```

Os cenários incluem:

```text
listagem
busca por ID
criação
situação padrão
trim
criação concluída
atualização
edição sem mudanças
conclusão
reabertura
exclusão lógica
exclusão permanente
```

Os testes automatizados têm como finalidade:

> Executar uma regra do sistema e verificar se o resultado obtido corresponde ao resultado esperado.

---

# 7. Resumo das Responsabilidades

## Frontend

### Funcionamento

Responsável por:

```text
interface
navegação
formulários
exibição
comunicação HTTP
feedback ao usuário
```

### Regra

Responsável por regras de experiência e apresentação.

Não deve concentrar regras críticas de negócio.

### Validação

Realiza validação imediata com React Hook Form e Zod antes de enviar dados.

### Comportamento

Controla:

```text
Dialogs
Menus
Loading
Erros
Snackbar
Tabela
Navegação
```

---

## Backend

### Funcionamento

Responsável pelo processamento das operações e persistência.

### Regra

É a fonte principal das regras de negócio.

### Validação

Protege a aplicação independentemente do frontend.

### Comportamento

Transforma requisições HTTP em operações sobre os dados e responde utilizando códigos HTTP apropriados.

---

# 8. Princípio Arquitetural

A separação adotada no projeto pode ser resumida por:

```text
Frontend
→ apresenta e coleta dados

Controller
→ entende HTTP

Service
→ entende regras de negócio

Repository
→ entende acesso aos dados

Entity Framework
→ traduz objetos para persistência

SQLite
→ armazena os registros
```

Essa divisão reduz acoplamento, facilita manutenção, permite testes isolados e possibilita a evolução futura da aplicação sem concentrar toda a responsabilidade em uma única camada.
