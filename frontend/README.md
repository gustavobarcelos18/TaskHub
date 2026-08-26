# Frontend ProjetoTarefas

Frontend Next.js (App Router), React, TypeScript, Material UI, React Hook Form e Zod para o projeto `ProjetoTarefas` em `backend/MinhaPrimeiraAPI`.

## Pré-requisitos

- Node.js e npm compatíveis com as dependências declaradas em `package.json`.
- Backend em execução em `https://localhost:7056`, com certificado de desenvolvimento confiável.

## Configuração

Copie o modelo de ambiente e ajuste a URL quando necessário:

```powershell
Copy-Item .env.example .env.local
```

```env
BACKEND_API_URL=https://localhost:7056
```

`BACKEND_API_URL` é privada ao servidor Next.js e indica o destino do rewrite `/api`. O navegador chama somente caminhos same-origin, como `/api/tarefas`.

Este projeto ignora `.env*` no Git, mas permite versionar `.env.example`. Use `.env.local` para valores específicos da máquina e mantenha o modelo sem segredos. Não há `.env` versionado atualmente.

## Comandos

```powershell
npm install
npm run dev
npm run lint
npm run build
```

`npm run dev` inicia o Next.js com HTTPS local. `npm run dev:api` inicia a API no perfil HTTPS e `npm run dev:all` inicia ambos. O Next.js encaminha `/api/*` para a API, sem CORS entre o navegador e a aplicação.

Antes do primeiro `npm run dev`, instale `mkcert` na máquina e execute `mkcert -install`. O comando falha em vez de iniciar em HTTP quando o certificado local não estiver disponível.

## Contrato consumido

Os tipos manuais, valores válidos e query parameters estão centralizados em `src/features/tarefas/types/tarefa.ts`; os payloads de formulário são validados em `schemas/tarefa-schema.ts`. A prioridade de API `Media` é apresentada como “Média” na interface. Datas de vencimento são datas civis: o frontend converte `dd/mm/aaaa` para `yyyy-MM-dd`, sem timezone.

Para detalhes de endpoints, erros, paginação, OpenAPI e banco, consulte o [README do backend](../../../backend/README.md).
