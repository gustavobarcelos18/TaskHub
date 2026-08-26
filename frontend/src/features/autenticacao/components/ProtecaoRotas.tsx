"use client";

import { Alert, Box, Button, CircularProgress, Stack } from "@mui/material";
import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import { useSessao } from "./SessaoProvider";

const rotasAutenticacao = new Set(["/login", "/cadastro"]);

export function ProtecaoRotas({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { estado, atualizarSessao } = useSessao();
  const ehRotaAutenticacao = rotasAutenticacao.has(pathname);

  useEffect(() => {
    if (estado.status === "anonimo" && !ehRotaAutenticacao) router.replace("/login");
    if (estado.status === "autenticado" && ehRotaAutenticacao) router.replace("/");
  }, [ehRotaAutenticacao, estado.status, router]);

  if (estado.status === "carregando") return <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center" }}><CircularProgress aria-label="Carregando sessão" /></Box>;
  if (estado.status === "erro") return <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center", p: 2 }}><Stack spacing={2}><Alert severity="error">Não foi possível verificar sua sessão.</Alert><Button variant="contained" onClick={() => void atualizarSessao()}>Tentar novamente</Button></Stack></Box>;
  if ((estado.status === "anonimo" && !ehRotaAutenticacao) || (estado.status === "autenticado" && ehRotaAutenticacao)) return null;
  return <>{children}</>;
}
