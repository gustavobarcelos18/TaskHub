import "server-only";
import { serverApiGet } from "@/features/autenticacao/services/server-api-client";
import type {
  ConsultaTarefas,
  Tarefa,
  TarefasPaginadas,
} from "../types/tarefa";

export async function listarTarefasServidor(
  consulta: ConsultaTarefas,
): Promise<TarefasPaginadas> {
  const parametros = new URLSearchParams();
  for (const [chave, valor] of Object.entries(consulta))
    if (valor !== undefined && valor !== null)
      parametros.set(chave, String(valor));
  const query = parametros.toString();
  return serverApiGet<TarefasPaginadas>(
    query ? `/api/tarefas?${query}` : "/api/tarefas",
  );
}

export function listarTarefasExcluidasServidor(): Promise<Tarefa[]> {
  return serverApiGet<Tarefa[]>("/api/tarefas/excluidas");
}
export function buscarTarefaServidor(id: number): Promise<Tarefa> {
  return serverApiGet<Tarefa>(`/api/tarefas/${id}`);
}
