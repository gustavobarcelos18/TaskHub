import type { Etiqueta } from "../types/tarefa";
import { requisicaoComAntiforgery } from "@/features/autenticacao/services/sessao-service";
import { criarErroHttp } from "@/services/criar-erro-http";

function obterUrlEtiquetas(): string {
  return "/api/etiquetas";
}

export async function listarEtiquetas(): Promise<Etiqueta[]> {
  const resposta = await fetch(obterUrlEtiquetas(), { cache: "no-store" });
  if (!resposta.ok) throw await criarErroHttp(resposta, "listar as etiquetas");
  return resposta.json() as Promise<Etiqueta[]>;
}

export async function criarEtiqueta(nome: string): Promise<Etiqueta> {
  const resposta = await requisicaoComAntiforgery(obterUrlEtiquetas(), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ nome }),
  });
  if (!resposta.ok) throw await criarErroHttp(resposta, "criar a etiqueta");
  return resposta.json() as Promise<Etiqueta>;
}

export async function excluirEtiqueta(id: number): Promise<void> {
  const resposta = await requisicaoComAntiforgery(
    `${obterUrlEtiquetas()}/${id}`,
    { method: "DELETE" },
  );
  if (!resposta.ok) throw await criarErroHttp(resposta, "excluir a etiqueta");
}
