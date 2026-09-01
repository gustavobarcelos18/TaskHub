import type { TarefaFormData } from "../schemas/tarefa-schema";
import { requisicaoComAntiforgery } from "@/features/autenticacao/services/sessao-service";
import { criarErroHttp } from "@/services/criar-erro-http";
import { converterDataCivilParaApi } from "../utils/formatar-data";
import type {
  ConsultaTarefas,
  HistoricoTarefa,
  Tarefa,
  TarefasPaginadas,
} from "../types/tarefa";

function obterUrlTarefas(): string {
  return "/api/tarefas";
}

export async function listarTarefas(
  consulta: ConsultaTarefas = {},
): Promise<TarefasPaginadas> {
  const parametros = new URLSearchParams();

  if (consulta.busca?.trim()) parametros.set("busca", consulta.busca.trim());
  if (consulta.situacao) parametros.set("situacao", consulta.situacao);
  if (consulta.prioridade) parametros.set("prioridade", consulta.prioridade);
  if (consulta.prazo) parametros.set("prazo", consulta.prazo);
  if (consulta.etiquetaId)
    parametros.set("etiquetaId", String(consulta.etiquetaId));
  if (consulta.projetoId)
    parametros.set("projetoId", String(consulta.projetoId));
  if (consulta.ordenarPor) parametros.set("ordenarPor", consulta.ordenarPor);
  if (consulta.direcao) parametros.set("direcao", consulta.direcao);
  if (consulta.pagina) parametros.set("pagina", String(consulta.pagina));
  if (consulta.tamanhoPagina)
    parametros.set("tamanhoPagina", String(consulta.tamanhoPagina));

  const url =
    parametros.size > 0
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
    throw await criarErroHttp(resposta, "carregar o histórico da tarefa");
  }

  return resposta.json() as Promise<HistoricoTarefa[]>;
}

export async function criarTarefa(dados: TarefaFormData): Promise<Tarefa> {
  const resposta = await requisicaoComAntiforgery(obterUrlTarefas(), {
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
  const resposta = await requisicaoComAntiforgery(
    `${obterUrlTarefas()}/${tarefaId}`,
    {
      method: "DELETE",
    },
  );

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "excluir a tarefa");
  }
}

export async function restaurarTarefa(tarefaId: number): Promise<void> {
  const resposta = await requisicaoComAntiforgery(
    `${obterUrlTarefas()}/${tarefaId}/restaurar`,
    {
      method: "PATCH",
    },
  );

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "restaurar a tarefa");
  }
}

export async function excluirTarefaPermanentemente(
  tarefaId: number,
): Promise<void> {
  const resposta = await requisicaoComAntiforgery(
    `${obterUrlTarefas()}/${tarefaId}/permanente`,
    {
      method: "DELETE",
    },
  );

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "excluir permanentemente a tarefa");
  }
}

export async function atualizarTarefa(
  tarefaId: number,
  dados: TarefaFormData,
): Promise<void> {
  const resposta = await requisicaoComAntiforgery(
    `${obterUrlTarefas()}/${tarefaId}`,
    {
      method: "PUT",

      headers: {
        "Content-Type": "application/json",
      },

      body: JSON.stringify({
        ...dados,
        dataVencimento: converterDataCivilParaApi(dados.dataVencimento),
      }),
    },
  );

  if (!resposta.ok) {
    throw await criarErroHttp(resposta, "atualizar a tarefa");
  }
}
