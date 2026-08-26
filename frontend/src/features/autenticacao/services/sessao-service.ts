export type UsuarioAutenticado = {
  id: string;
  email: string;
};

type TokenAntiforgery = { token: string };

let tokenAntiforgery: string | null = null;

async function obterTokenAntiforgery(): Promise<string> {
  if (tokenAntiforgery) return tokenAntiforgery;
  const resposta = await fetch("/api/autenticacao/antiforgery", { cache: "no-store", credentials: "same-origin" });
  if (!resposta.ok) throw new Error("Não foi possível preparar a requisição com segurança.");
  tokenAntiforgery = (await resposta.json() as TokenAntiforgery).token;
  return tokenAntiforgery;
}

export function invalidarTokenAntiforgery(): void { tokenAntiforgery = null; }

export async function requisicaoComAntiforgery(url: string, init: RequestInit): Promise<Response> {
  const token = await obterTokenAntiforgery();
  return fetch(url, { ...init, credentials: "same-origin", headers: { ...init.headers, "X-CSRF-TOKEN": token } });
}

async function erro(resposta: Response): Promise<Error> {
  if (resposta.status >= 500) return new Error("O serviço está indisponível. Tente novamente em instantes.");
  const conteudo: unknown = await resposta.json().catch(() => null);
  const problema = conteudo && typeof conteudo === "object" ? conteudo as { detail?: unknown; title?: unknown } : null;
  return new Error(typeof problema?.detail === "string" ? problema.detail : typeof problema?.title === "string" ? problema.title : "Não foi possível concluir a operação.");
}

export async function obterSessao(): Promise<UsuarioAutenticado | null> {
  const resposta = await fetch("/api/autenticacao/sessao", { cache: "no-store", credentials: "same-origin" });
  if (resposta.status === 401) return null;
  if (!resposta.ok) throw await erro(resposta);
  return resposta.json() as Promise<UsuarioAutenticado>;
}

export async function login(email: string, senha: string): Promise<UsuarioAutenticado> {
  const resposta = await requisicaoComAntiforgery("/api/autenticacao/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email, senha }) });
  if (!resposta.ok) throw await erro(resposta);
  invalidarTokenAntiforgery();
  return resposta.json() as Promise<UsuarioAutenticado>;
}

export async function cadastrar(email: string, senha: string): Promise<UsuarioAutenticado> {
  const resposta = await requisicaoComAntiforgery("/api/autenticacao/cadastro", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email, senha }) });
  if (!resposta.ok) throw await erro(resposta);
  invalidarTokenAntiforgery();
  return resposta.json() as Promise<UsuarioAutenticado>;
}

export async function logout(): Promise<void> {
  const resposta = await requisicaoComAntiforgery("/api/autenticacao/logout", { method: "POST" });
  if (!resposta.ok) throw await erro(resposta);
  invalidarTokenAntiforgery();
}
