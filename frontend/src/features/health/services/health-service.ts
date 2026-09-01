import type { HealthDetails } from "../types/health";

export async function obterHealthDetalhado(signal?: AbortSignal): Promise<HealthDetails> {
  const response = await fetch("/api/health/detalhes", { cache: "no-store", signal });
  if (!response.ok) throw new Error("Não foi possível consultar a API.");
  return response.json() as Promise<HealthDetails>;
}
