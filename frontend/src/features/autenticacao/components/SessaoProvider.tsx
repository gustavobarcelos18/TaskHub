"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { obterSessao, type UsuarioAutenticado } from "../services/sessao-service";

export type EstadoSessao = { status: "carregando" } | { status: "anonimo" } | { status: "autenticado"; usuario: UsuarioAutenticado } | { status: "erro" };

type ContextoSessao = { estado: EstadoSessao; atualizarSessao: () => Promise<void>; definirAnonimo: () => void };
const SessaoContext = createContext<ContextoSessao | null>(null);

export function SessaoProvider({ children }: { children: React.ReactNode }) {
  const [estado, setEstado] = useState<EstadoSessao>({ status: "carregando" });
  const consultaEmAndamento = useRef<Promise<void> | null>(null);
  const atualizarSessao = useCallback(async () => {
    if (consultaEmAndamento.current) return consultaEmAndamento.current;
    consultaEmAndamento.current = obterSessao()
      .then((usuario) => setEstado(usuario ? { status: "autenticado", usuario } : { status: "anonimo" }))
      .catch((erro) => { setEstado({ status: "erro" }); throw erro; })
      .finally(() => { consultaEmAndamento.current = null; });
    return consultaEmAndamento.current;
  }, []);
  useEffect(() => { void atualizarSessao().catch(() => undefined); }, [atualizarSessao]);
  const valor = useMemo(() => ({ estado, atualizarSessao, definirAnonimo: () => setEstado({ status: "anonimo" }) }), [estado, atualizarSessao]);
  return <SessaoContext.Provider value={valor}>{children}</SessaoContext.Provider>;
}

export function useSessao(): ContextoSessao {
  const contexto = useContext(SessaoContext);
  if (!contexto) throw new Error("useSessao deve ser usado dentro de SessaoProvider.");
  return contexto;
}
