# Frontend ProjetoTarefas

Frontend Next.js (App Router), React, TypeScript, Material UI, React Hook Form e Zod para a API em `backend/MinhaPrimeiraAPI`.

## Pré-requisitos

- Node.js e npm compatíveis com as dependências declaradas em `package.json`.
- Backend em execução, normalmente em `http://localhost:5025`.

## Configuração

Copie o modelo de ambiente e ajuste a URL quando necessário:

```powershell
Copy-Item .env.example .env.local
```

```env
NEXT_PUBLIC_API_URL=http://localhost:5025
```

`NEXT_PUBLIC_API_URL` é obrigatória e é usada pelo service HTTP em `src/features/tarefas/services/tarefa-service.ts` para formar as rotas `/api/tarefas`.

Este projeto ignora `.env*` no Git, mas permite versionar `.env.example`. Use `.env.local` para valores específicos da máquina e mantenha o modelo sem segredos. Não há `.env` versionado atualmente.

## Comandos

```powershell
npm install
npm run dev
npm run lint
npm run build
```

`npm run dev:api` inicia a API .NET a partir do frontend e `npm run dev:all` inicia API e Next.js juntos. O backend permite, por padrão, a origem `http://localhost:3000`; ajuste `Cors:AllowedOrigins` no backend se executar o frontend em outra origem.

## Contrato consumido

Os tipos manuais, valores válidos e query parameters estão centralizados em `src/features/tarefas/types/tarefa.ts`; os payloads de formulário são validados em `schemas/tarefa-schema.ts`. A prioridade de API `Media` é apresentada como “Média” na interface. Datas de vencimento são datas civis: o frontend converte `dd/mm/aaaa` para `yyyy-MM-dd`, sem timezone.

Para detalhes de endpoints, erros, paginação, OpenAPI e banco, consulte o [README do backend](../../../backend/README.md).
