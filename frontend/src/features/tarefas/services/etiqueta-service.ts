import type { Etiqueta } from "../types/tarefa";
import { requisicaoComAntiforgery } from "@/features/autenticacao/services/sessao-service";

function obterUrlEtiquetas(): string {
  return "/api/etiquetas";
}

async function erro(resposta: Response, operacao: string): Promise<Error> {
  const conteudo: unknown = await resposta.json().catch(() => null);
  const problema = conteudo && typeof conteudo === "object" ? conteudo as { detail?: unknown; title?: unknown } : null;
  return new Error(typeof problema?.detail === "string" ? problema.detail : typeof problema?.title === "string" ? problema.title : `Não foi possível ${operacao}.`);
}

export async function listarEtiquetas(): Promise<Etiqueta[]> {
  const resposta = await fetch(obterUrlEtiquetas(), { cache: "no-store" });
  if (!resposta.ok) throw await erro(resposta, "listar as etiquetas");
  return resposta.json() as Promise<Etiqueta[]>;
}

export async function criarEtiqueta(nome: string): Promise<Etiqueta> {
  const resposta = await requisicaoComAntiforgery(obterUrlEtiquetas(), { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ nome }) });
  if (!resposta.ok) throw await erro(resposta, "criar a etiqueta");
  return resposta.json() as Promise<Etiqueta>;
}

export async function excluirEtiqueta(id: number): Promise<void> {
  const resposta = await requisicaoComAntiforgery(`${obterUrlEtiquetas()}/${id}`, { method: "DELETE" });
  if (!resposta.ok) throw await erro(resposta, "excluir a etiqueta");
}
