import type { TarefaFormData } from "../schemas/tarefa-schema";
import type { Tarefa } from "../types/tarefa";

const apiUrl = process.env.NEXT_PUBLIC_API_URL;

if (!apiUrl) {
  throw new Error("A variável NEXT_PUBLIC_API_URL não foi configurada.");
}

export async function listarTarefas(): Promise<Tarefa[]> {
  const resposta = await fetch(`${apiUrl}/api/tarefas`, {
    cache: "no-store",
  });

  if (!resposta.ok) {
    throw new Error(
      `Não foi possível listar as tarefas. Status: ${resposta.status}`,
    );
  }

  const tarefas: Tarefa[] = await resposta.json();

  return tarefas;
}

export async function criarTarefa(dados: TarefaFormData): Promise<Tarefa> {
  const resposta = await fetch(`${apiUrl}/api/tarefas`, {
    method: "POST",

    headers: {
      "Content-Type": "application/json",
    },

    body: JSON.stringify(dados),
  });

  if (!resposta.ok) {
    const conteudoErro = await resposta.text();

    throw new Error(
      `Não foi possível cadastrar a tarefa. Status: ${resposta.status}. Resposta: ${conteudoErro || "sem conteúdo"}`,
    );
  }

  const conteudoResposta = await resposta.text();

  if (!conteudoResposta) {
    throw new Error(
      "A API confirmou o cadastro, mas não devolveu os dados da tarefa criada.",
    );
  }

  const tarefaCriada: Tarefa = JSON.parse(conteudoResposta);

  return tarefaCriada;
}

export async function excluirTarefa(tarefaId: number): Promise<void> {
  const resposta = await fetch(`${apiUrl}/api/tarefas/${tarefaId}`, {
    method: "DELETE",
  });

  if (!resposta.ok) {
    const conteudoErro = await resposta.text();

    throw new Error(
      `Não foi possível excluir a tarefa. Status: ${resposta.status}. Resposta: ${conteudoErro || "sem conteúdo"}`,
    );
  }
}

export async function atualizarTarefa(
  tarefaId: number,
  dados: TarefaFormData,
): Promise<void> {
  const resposta = await fetch(`${apiUrl}/api/tarefas/${tarefaId}`, {
    method: "PUT",

    headers: {
      "Content-Type": "application/json",
    },

    body: JSON.stringify(dados),
  });

  if (!resposta.ok) {
    const conteudoErro = await resposta.text();

    throw new Error(
      `Não foi possível atualizar a tarefa. Status: ${resposta.status}. Resposta: ${conteudoErro || "sem conteúdo"}`,
    );
  }
}
