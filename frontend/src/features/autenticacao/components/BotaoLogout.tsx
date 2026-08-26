"use client";

import LogoutIcon from "@mui/icons-material/Logout";
import Button from "@mui/material/Button";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { logout } from "../services/sessao-service";
import { useSessao } from "./SessaoProvider";

export function BotaoLogout() {
  const [processando, setProcessando] = useState(false); const router = useRouter(); const { definirAnonimo } = useSessao();
  const sair = async () => { try { setProcessando(true); await logout(); definirAnonimo(); router.replace("/login"); router.refresh(); } finally { setProcessando(false); } };
  return <Button color="inherit" startIcon={<LogoutIcon />} disabled={processando} onClick={() => void sair()}>{processando ? "Saindo..." : "Sair"}</Button>;
}
