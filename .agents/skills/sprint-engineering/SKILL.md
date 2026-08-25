---
name: sprint-engineering
description: Executa uma Sprint do ProjetoTarefas de forma incremental, validada e sem antecipar escopo futuro.
---

# Sprint Engineering Workflow

## Quando usar

Use quando o usuário solicitar implementar uma Sprint, desenvolver um conjunto incremental de funcionalidades, executar um roadmap ou entregar uma melhoria funcional delimitada.

## Pré-condição

Antes de qualquer mudança:

- leia o `AGENTS.md`;
- leia o prompt da Sprint;
- inspecione o código real;
- não assuma estado baseado somente no prompt.

## Workflow obrigatório

### 1. INSPECT

- Identifique arquivos envolvidos e entenda o fluxo atual.
- Procure funcionalidade já existente, analise os testes relevantes e o contrato frontend/backend.
- Identifique impactos e riscos.

Não altere código nesta etapa.

### 2. BASELINE

Execute e registre os resultados reais:

```powershell
git status
dotnet build ProjetoTarefas.slnx
dotnet test ProjetoTarefas.slnx --no-build
```

No diretório `frontend/minha-primeira-api-web`, execute:

```powershell
npm run lint
npm run build
```

Se houver falhas antes da Sprint, não as esconda: diferencie-as de regressões introduzidas depois.

### 3. PLAN

Antes de editar, produza um plano curto com:

- backend;
- frontend;
- testes;
- arquivos prováveis;
- riscos;
- itens explicitamente fora do escopo.

Não crie arquitetura excessiva.

### 4. IMPLEMENT BACKEND

Quando aplicável, implemente preferencialmente nesta ordem:

- contrato/DTO;
- repository;
- service;
- controller;
- testes.

Faça mudanças pequenas e coerentes. Em seguida, execute o build e os testes do backend. Não avance sobre regressões não resolvidas.

### 5. IMPLEMENT FRONTEND

Quando aplicável, implemente tipos, service HTTP, página, componentes, navegação e estados. Em seguida, execute lint e build do frontend.

### 6. FUNCTIONAL VALIDATION

Quando o comportamento for alterado:

- inicie a API recém-compilada;
- teste endpoints reais;
- use dados temporários com prefixo da Sprint;
- verifique status HTTP e efeitos reais;
- confira o banco quando necessário;
- remova os dados criados.

Nunca use registros normais do usuário como massa destrutiva de teste.

### 7. VISUAL VALIDATION

Com navegador disponível, teste os fluxos de UI alterados. Sem navegador, registre exatamente: `NÃO EXECUTADO VISUALMENTE.`

### 8. SELF REVIEW

Revise `git diff` e releia os arquivos alterados. Procure redundância, código morto, complexidade desnecessária, inconsistências de contrato, lógica dependente de efeito colateral, validações duplicadas, estados mortos, queries ineficientes e dependências desnecessárias. Corrija apenas problemas reais dentro do escopo.

### 9. REGRESSION CHECK

Faça smoke tests focados nas áreas afetadas para confirmar que funcionalidades relacionadas continuam funcionando. Não repita toda a suíte funcional histórica quando isso não for necessário.

### 10. FINAL VALIDATION

Execute novamente:

```powershell
dotnet build ProjetoTarefas.slnx
dotnet test ProjetoTarefas.slnx --no-build
```

E, quando o frontend for alterado:

```powershell
npm run lint
npm run build
```

Registre os resultados finais.

### 11. GIT REVIEW

Execute:

```powershell
git status
git diff --stat
```

Não faça commit automaticamente.

### 12. REPORT

Entregue: resumo da Sprint; baseline; arquivos criados e alterados; backend e frontend; regras e testes novos; totais de testes antes/depois; testes funcionais e visuais; build, testes, lint e build frontend; regressões verificadas; performance quando aplicável; problemas encontrados e fora do escopo; dados temporários e limpeza; e status final do Git.

## Critério de encerramento

Uma Sprint só termina quando os requisitos e critérios do prompt forem verificados, testes anteriores continuarem passando, testes novos necessários forem adicionados, backend e frontend alterados forem validados, dados temporários forem removidos, regressões relevantes forem verificadas e nenhuma funcionalidade futura for antecipada sem autorização.

## Escopo

Esta skill define **como** executar uma Sprint. Requisitos funcionais pertencem exclusivamente ao prompt daquela Sprint; regras permanentes pertencem ao `AGENTS.md`.
