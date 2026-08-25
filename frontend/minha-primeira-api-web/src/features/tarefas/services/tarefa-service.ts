import type { TarefaFormData } from "../schemas/tarefa-schema";
import { converterDataCivilParaApi } from "../utils/formatar-data";
import type {
  ConsultaTarefas,
  HistoricoTarefa,
  ResumoTarefas,
  Tarefa,
  TarefasPaginadas,
} from "../types/tarefa";

type ApiProblemDetails = {
  detail?: string;
  errors?: Record<string, string[]>;
  instance?: string;
  status?: number;
  title?: string;
  traceId?: string;
  type?: string;
};

function obterUrlTarefas(): string {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL;

  if (!apiUrl) {
    throw new Error("A variável NEXT_PUBLIC_API_URL não foi configurada.");
  }

  return `${apiUrl}/api/tarefas`;
}

async function criarErroHttp(
  resposta: Response,
  operacao: string,
): Promise<Error> {
  const problema = await obterProblemDetails(resposta);
  const mensagemValidacao = problema?.errors
    ? Object.values(problema.errors).flat().find(Boolean)
    : undefined;
  const mensagem = mensagemValidacao ?? problema?.detail ?? problema?.title;

  return new Error(
    mensagem ?? `Não foi possível ${operacao}. Status: ${resposta.status}.`,
  );
}

async function obterProblemDetails(
  resposta: Response,
): Promise<ApiProblemDetails | null> {
  if (!resposta.headers.get("content-type")?.includes("application/json")) {
    return null;
  }

  const conteudo: unknown = await resposta.json().catch(() => null);

  return ehApiProblemDetails(conteudo) ? conteudo : null;
}

function ehApiProblemDetails(valor: unknown): valor is ApiProblemDetails {
  if (!valor || typeof valor !== "object") return false;

  const problema = valor as Record<string, unknown>;

  return (
    (problema.detail === undefined || typeof problema.detail === "string") &&
    (problema.title === undefined || typeof problema.title === "string") &&
    (problema.errors === undefined || errosSaoValidos(problema.errors))
  );
}

function errosSaoValidos(valor: unknown): valor is Record<string, string[]> {
  if (!valor || typeof valor !== "object") return false;

  return Object.values(valor).every(
    (mensagens) =>
      Array.isArray(mensagens) &&
      mensagens.every((mensagem) => typeof mensagem === "string"),
  );
}

export async function listarTarefas(
  consulta: ConsultaTarefas = {},
): Promise<TarefasPaginadas> {
  const parametros = new URLSearchParams();

  if (consulta.busca?.trim()) parametros.set("busca", consulta.busca.trim());
  if (consulta.situacao) parametros.set("situacao", consulta.situacao);
  if (consulta.prioridade) parametros.set("prioridade", consulta.prioridade);
  if (consulta.prazo) parametros.set("prazo", consulta.prazo);
  if (consulta.ordenarPor) parametros.set("ordenarPor", consulta.ordenarPor);
  if (consulta.direcao) parametros.set("direcao", consulta.direcao);
  if (consulta.pagina) parametros.set("pagina", String(consulta.pagina));
  if (consulta.tamanhoPagina) parametros.set("tamanhoPagina", String(consulta.tamanhoPagina));

  const url = parametros.size > 0
    ? `${obterUrlTarefas()}?${parametros.toString()}`
    : obterUrlTarefas();

  const resposta = await fetch(url, {
    cache: "no-store",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "listar as tarefas");
  }

  const tarefas: TarefasPaginadas = await resposta.json();

  return tarefas;
}

export async function obterResumoTarefas(): Promise<ResumoTarefas> {
  const resposta = await fetch(`${obterUrlTarefas()}/resumo`, {
    cache: "no-store",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "carregar o resumo das tarefas");
  }

  return resposta.json() as Promise<ResumoTarefas>;
}

export async function listarTarefasExcluidas(): Promise<Tarefa[]> {
  const resposta = await fetch(`${obterUrlTarefas()}/excluidas`, {
    cache: "no-store",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "listar as tarefas excluídas");
  }

  return resposta.json() as Promise<Tarefa[]>;
}

export async function buscarTarefa(tarefaId: number): Promise<Tarefa> {
  const resposta = await fetch(`${obterUrlTarefas()}/${tarefaId}`, {
    cache: "no-store",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "carregar os detalhes da tarefa");
  }

  return resposta.json() as Promise<Tarefa>;
}

export async function listarHistoricoTarefa(
  tarefaId: number,
): Promise<HistoricoTarefa[]> {
  const resposta = await fetch(`${obterUrlTarefas()}/${tarefaId}/historico`, {
    cache: "no-store",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "carregar o histÃ³rico da tarefa");
  }

  return resposta.json() as Promise<HistoricoTarefa[]>;
}

export async function criarTarefa(dados: TarefaFormData): Promise<Tarefa> {
  const resposta = await fetch(obterUrlTarefas(), {
    method: "POST",

    headers: {
      "Content-Type": "application/json",
    },

    body: JSON.stringify({
      ...dados,
      dataVencimento: converterDataCivilParaApi(dados.dataVencimento),
    }),
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "cadastrar a tarefa");
  }

  return resposta.json() as Promise<Tarefa>;
}

export async function excluirTarefa(tarefaId: number): Promise<void> {
  const resposta = await fetch(`${obterUrlTarefas()}/${tarefaId}`, {
    method: "DELETE",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "excluir a tarefa");
  }
}

export async function restaurarTarefa(tarefaId: number): Promise<void> {
  const resposta = await fetch(`${obterUrlTarefas()}/${tarefaId}/restaurar`, {
    method: "PATCH",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "restaurar a tarefa");
  }
}

export async function excluirTarefaPermanentemente(
  tarefaId: number,
): Promise<void> {
  const resposta = await fetch(`${obterUrlTarefas()}/${tarefaId}/permanente`, {
    method: "DELETE",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "excluir permanentemente a tarefa");
  }
}

export async function atualizarTarefa(
  tarefaId: number,
  dados: TarefaFormData,
): Promise<void> {
  const resposta = await fetch(`${obterUrlTarefas()}/${tarefaId}`, {
    method: "PUT",

    headers: {
      "Content-Type": "application/json",
    },

    body: JSON.stringify({
      ...dados,
      dataVencimento: converterDataCivilParaApi(dados.dataVencimento),
    }),
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "atualizar a tarefa");
  }
}
