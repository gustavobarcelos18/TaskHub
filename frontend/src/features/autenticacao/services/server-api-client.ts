import "server-only";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";

const backendUrl = process.env.BACKEND_API_URL;

export async function serverApiGet<T>(caminho: string): Promise<T> {
  if (!backendUrl) throw new Error("BACKEND_API_URL não está configurada.");
  const cookieSessao = (await cookies()).get("__Host-taskhub")?.value;
  const resposta = await fetch(`${backendUrl}${caminho}`, {
    cache: "no-store",
    headers: cookieSessao ? { Cookie: `__Host-taskhub=${cookieSessao}` } : undefined,
  });
  if (resposta.status === 401) redirect("/login");
  if (!resposta.ok) throw new Error(`Falha ao carregar dados. Status: ${resposta.status}.`);
  return resposta.json() as Promise<T>;
}
