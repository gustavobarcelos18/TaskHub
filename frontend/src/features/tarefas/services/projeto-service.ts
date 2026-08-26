import type { Projeto } from "../types/tarefa";
import { requisicaoComAntiforgery } from "@/features/autenticacao/services/sessao-service";

function obterUrlProjetos(): string {
  return "/api/projetos";
}

async function erro(resposta: Response, operacao: string): Promise<Error> {
  const conteudo: unknown = await resposta.json().catch(() => null);
  const problema = conteudo && typeof conteudo === "object" ? conteudo as { detail?: unknown; title?: unknown } : null;
  return new Error(typeof problema?.detail === "string" ? problema.detail : typeof problema?.title === "string" ? problema.title : `Não foi possível ${operacao}.`);
}

export async function listarProjetos(): Promise<Projeto[]> {
  const resposta = await fetch(obterUrlProjetos(), { cache: "no-store" });
  if (!resposta.ok) throw await erro(resposta, "listar os projetos");
  return resposta.json() as Promise<Projeto[]>;
}

export async function criarProjeto(nome: string): Promise<Projeto> {
  const resposta = await requisicaoComAntiforgery(obterUrlProjetos(), { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ nome }) });
  if (!resposta.ok) throw await erro(resposta, "criar o projeto");
  return resposta.json() as Promise<Projeto>;
}

export async function excluirProjeto(id: number): Promise<void> {
  const resposta = await requisicaoComAntiforgery(`${obterUrlProjetos()}/${id}`, { method: "DELETE" });
  if (!resposta.ok) throw await erro(resposta, "excluir o projeto");
}
