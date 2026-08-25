# ProjetoTarefas — Engineering Rules

## 1. Fonte de verdade

- O código e a estrutura atuais são a fonte de verdade.
- Antes de alterar algo, inspecione os arquivos, contratos DTO, serviços e testes envolvidos.
- Não presuma que documentação antiga representa exatamente o estado atual e não recrie funcionalidades existentes.
- Preserve a arquitetura existente, salvo solicitação explícita, e não modifique código não relacionado.

## 2. Arquitetura

O backend segue `Controller → Service → Repository → AppDbContext → SQLite`.

- **Controller:** contrato HTTP, model binding e status HTTP; sem lógica de negócio relevante.
- **Service:** regras de negócio, validações, normalização e orquestração.
- **Repository:** consultas, persistência e EF Core; sem lógica HTTP.
- **AppDbContext:** mappings, query filters e configuração do EF.

O frontend segue `Page → Components → Service HTTP → API`. Preserve o App Router e a separação atual. Não altere a arquitetura apenas por preferência.

## 3. Simplicidade

- Prefira implementações simples, explícitas, testáveis e fáceis de manter.
- Não crie abstrações, helpers para uma única chamada trivial ou interfaces sem benefício concreto.
- Evite CQRS, MediatR, AutoMapper, UnitOfWork adicional, arquitetura hexagonal, Clean Architecture completa, Redux, TanStack Query e Axios, salvo pedido específico.

## 4. Backend e C#

- Use injeção de dependência, preserve nullable reference types e use `async` quando apropriado.
- Mantenha métodos pequenos, nomes claros e fluxo linear; evite código excessivamente compacto, reflexão e comentários que só repetem o código.
- Chamadas curtas devem permanecer em uma linha quando isso preservar a legibilidade.
- Não exponha entidades EF pela API quando houver contrato DTO.

## 5. Entity Framework e SQLite

- Consultas somente leitura devem preferir `AsNoTracking`.
- Aplique filtros, ordenação e paginação no banco; não materialize ou filtre em memória sem necessidade.
- Evite N+1 e não duplique um `HasQueryFilter` com `Where` equivalente sem motivo.
- Preserve o filtro global de exclusão lógica; use `IgnoreQueryFilters` somente quando necessário.
- Não recrie banco, apague dados, altere migrations antigas, consolide migrations, remova migrations corretivas ou modifique o snapshot manualmente sem necessidade.
- Crie migration apenas para mudança real de schema. Não use EF InMemory para simular comportamento relacional.

## 6. Frontend

- Material UI é a biblioteca visual padrão. Preserve o Theme e prefira seus tokens a cores hardcoded.
- Não reintroduza Tailwind, Base UI ou Lucide; use componentes MUI adequados quando existirem.
- Mantenha comunicação HTTP em services e use `fetch` nativo.
- Preserve TypeScript estrito; evite `any`, estado duplicado e `useEffect` desnecessário.
- Mantenha Server Components quando possível e use Client Components apenas quando necessário.

## 7. Formulários e contratos

- Preserve React Hook Form + Zod nos formulários; não duplique validações manuais.
- Reutilize schemas, constantes e listas de valores válidos existentes quando apropriado.
- Frontend e backend devem manter contratos consistentes. O backend protege regras de negócio; o frontend não é a única validação.
- Não leve cálculos de negócio ao frontend nem baixe todos os dados para filtrar, ordenar, contar ou paginar quando isso pertence ao banco.

## 8. Datas

- O backend usa UTC como referência e o frontend apresenta datas pelo utilitário existente.
- Não aplique offsets manuais, preserve a semântica das datas de auditoria e não altere datas sem relação com a operação.

## 9. Dependências

- Não adicione pacote NuGet ou npm sem necessidade real; confirme primeiro que a stack atual não resolve o problema.
- Não atualize versões como efeito colateral. Justifique qualquer nova dependência no relatório.

## 10. Testes e validação

- Registre um baseline antes de mudanças relevantes, preserve testes existentes e acrescente testes para regras novas quando justificável.
- Não altere testes para esconder regressões nem crie testes artificiais para aumentar a contagem.
- Não crie testes de integração sem instrução explícita; não adicione automaticamente `WebApplicationFactory`, `TestServer`, EF InMemory, Playwright ou Cypress.
- Testes unitários não devem fingir provar comportamento SQL que exige SQLite real.

Para validar o backend, execute:

```powershell
dotnet build ProjetoTarefas.slnx
dotnet test ProjetoTarefas.slnx --no-build
```

Registre total, aprovados, falhas, ignorados, warnings e erros. Ausência de erro no editor não é validação.

Para validar o frontend, no diretório `frontend/minha-primeira-api-web`, execute:

```powershell
npm run lint
npm run build
```

O build só é aprovado quando concluir normalmente. Documente erro ambiental sem declarar sucesso ou alterar código correto para contorná-lo.

## 11. Testes funcionais e visuais

- Quando houver mudança funcional, teste a API real quando possível, com dados temporários claramente identificáveis, e remova-os ao final.
- Não use registros normais do usuário como massa destrutiva e diferencie testes unitários de funcionais.
- Sem navegador, registre exatamente: `NÃO EXECUTADO VISUALMENTE.` Nunca invente teste visual.

## 12. Git e escopo

- São permitidos `git status`, `git diff`, `git diff --stat`, `git diff --cached` e `git grep`.
- Não execute automaticamente `git commit`, `git push`, `git reset --hard` ou `git clean -fd`; não apague arquivos ignorados.
- Não antecipe funcionalidades de Sprints futuras nem refatore áreas não relacionadas. Documente problemas fora do escopo e só os corrija se bloquearem a tarefa, com justificativa.

## 13. Revisão obrigatória

Após implementar, releia os arquivos alterados e revise o diff procurando código morto, duplicação, branches impossíveis, imports ou estado React mortos, fetches e `router.refresh` redundantes, `SaveChanges` desnecessário, tracking incorreto, queries redundantes, validação duplicada, strings mágicas e abstrações artificiais.

## 14. Três níveis de validação

- **EXECUTOU:** o comando ou fluxo foi executado.
- **TEVE O EFEITO ESPERADO:** o resultado esperado foi observado.
- **ESTÁ CONSISTENTE:** o resultado foi comparado às regras e a possíveis regressões.

Não trate esses níveis como equivalentes.

## 15. Linguagem do relatório e regra final

- Não declare “100% sem bugs”, “garantidamente perfeito” ou equivalentes.
- Prefira afirmações verificáveis, como “todos os testes existentes passaram”, “o build concluiu sem erros” e “não foram observadas regressões nos cenários verificados”.
- Não altere código apenas para demonstrar atividade. Uma parte correta pode permanecer sem mudanças.
- Priorize clareza, consistência, simplicidade, testabilidade, manutenção e ausência de regressões conhecidas.

## 16. Diário de trabalho

- Mantenha o arquivo `DIARIO.md` atualizado a cada tarefa ou etapa relevante concluída.
- Registre a data, as alterações e atualizações realizadas, as validações executadas e eventuais pendências ou limitações.
- Não omita alterações feitas no repositório durante o trabalho. Preserve os registros anteriores e adicione novos itens em ordem cronológica.
