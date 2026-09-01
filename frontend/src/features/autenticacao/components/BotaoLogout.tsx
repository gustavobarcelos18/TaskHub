"use client";

import LogoutIcon from "@mui/icons-material/Logout";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Snackbar from "@mui/material/Snackbar";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { logout } from "../services/sessao-service";
import { useSessao } from "./SessaoProvider";

export function BotaoLogout() {
  const [processando, setProcessando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const router = useRouter();
  const { definirAnonimo } = useSessao();

  async function sair() {
    try {
      setProcessando(true);
      setErro(null);
      await logout();
      definirAnonimo();
      router.replace("/login");
      router.refresh();
    } catch (causa) {
      setErro(
        causa instanceof Error
          ? causa.message
          : "Não foi possível encerrar a sessão.",
      );
    } finally {
      setProcessando(false);
    }
  }

  return (
    <>
      <Button
        color="inherit"
        startIcon={<LogoutIcon />}
        disabled={processando}
        onClick={() => void sair()}
      >
        {processando ? "Saindo..." : "Sair"}
      </Button>
      <Snackbar
        open={erro !== null}
        autoHideDuration={5000}
        onClose={() => setErro(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert severity="error" variant="filled" onClose={() => setErro(null)}>
          {erro}
        </Alert>
      </Snackbar>
    </>
  );
}
