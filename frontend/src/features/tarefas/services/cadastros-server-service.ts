import "server-only";
import { serverApiGet } from "@/features/autenticacao/services/server-api-client";
import type { Etiqueta, Projeto } from "../types/tarefa";
export function listarEtiquetasServidor(): Promise<Etiqueta[]> { return serverApiGet<Etiqueta[]>("/api/etiquetas"); }
export function listarProjetosServidor(): Promise<Projeto[]> { return serverApiGet<Projeto[]>("/api/projetos"); }
