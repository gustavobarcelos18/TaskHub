import type { Projeto } from "../types/tarefa";
import { requisicaoComAntiforgery } from "@/features/autenticacao/services/sessao-service";
import { criarErroHttp } from "@/services/criar-erro-http";

function obterUrlProjetos(): string {
  return "/api/projetos";
}

export async function listarProjetos(): Promise<Projeto[]> {
  const resposta = await fetch(obterUrlProjetos(), { cache: "no-store" });
  if (!resposta.ok) throw await criarErroHttp(resposta, "listar os projetos");
  return resposta.json() as Promise<Projeto[]>;
}

export async function criarProjeto(nome: string): Promise<Projeto> {
  const resposta = await requisicaoComAntiforgery(obterUrlProjetos(), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ nome }),
  });
  if (!resposta.ok) throw await criarErroHttp(resposta, "criar o projeto");
  return resposta.json() as Promise<Projeto>;
}

export async function excluirProjeto(id: number): Promise<void> {
  const resposta = await requisicaoComAntiforgery(
    `${obterUrlProjetos()}/${id}`,
    { method: "DELETE" },
  );
  if (!resposta.ok) throw await criarErroHttp(resposta, "excluir o projeto");
}
