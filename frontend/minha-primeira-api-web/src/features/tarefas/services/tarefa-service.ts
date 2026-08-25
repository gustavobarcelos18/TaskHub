import type { TarefaFormData } from "../schemas/tarefa-schema";
import type { Tarefa } from "../types/tarefa";

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
  const conteudoErro = await resposta.text();

  return new Error(
    `Não foi possível ${operacao}. Status: ${resposta.status}. Resposta: ${conteudoErro || "sem conteúdo"}`,
  );
}

export async function listarTarefas(): Promise<Tarefa[]> {
  const resposta = await fetch(obterUrlTarefas(), {
    cache: "no-store",
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "listar as tarefas");
  }

  const tarefas: Tarefa[] = await resposta.json();

  return tarefas;
}

export async function criarTarefa(dados: TarefaFormData): Promise<Tarefa> {
  const resposta = await fetch(obterUrlTarefas(), {
    method: "POST",

    headers: {
      "Content-Type": "application/json",
    },

    body: JSON.stringify(dados),
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

export async function atualizarTarefa(
  tarefaId: number,
  dados: TarefaFormData,
): Promise<void> {
  const resposta = await fetch(`${obterUrlTarefas()}/${tarefaId}`, {
    method: "PUT",

    headers: {
      "Content-Type": "application/json",
    },

    body: JSON.stringify(dados),
  });

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "atualizar a tarefa");
  }
}
