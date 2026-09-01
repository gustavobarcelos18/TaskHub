"use client";

import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import RefreshIcon from "@mui/icons-material/Refresh";
import Link from "next/link";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Container,
  Grid,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import { useCallback, useEffect, useRef, useState } from "react";
import { obterHealthDetalhado } from "@/features/health/services/health-service";
import type { HealthCheck, HealthDetails, HealthStatus } from "@/features/health/types/health";

const nomes: Record<string, string> = {
  api: "API ASP.NET Core",
  sqlite: "SQLite",
  persistence: "Persistência / backend ↔ banco",
  filesystem: "Filesystem",
};

const statusLabels: Record<HealthStatus, string> = {
  Healthy: "Saudável",
  Degraded: "Degradado",
  Unhealthy: "Indisponível",
};

function statusColor(status: HealthStatus): "success" | "warning" | "error" {
  return status === "Healthy" ? "success" : status === "Degraded" ? "warning" : "error";
}

function formatarUptime(seconds: number): string {
  const dias = Math.floor(seconds / 86400);
  const horas = Math.floor((seconds % 86400) / 3600);
  const minutos = Math.floor((seconds % 3600) / 60);
  return dias > 0 ? `${dias}d ${horas}h ${minutos}min` : `${horas}h ${minutos}min`;
}

function formatarData(data: string): string {
  return new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "medium" }).format(new Date(data));
}

function CheckCard({ check }: { check: HealthCheck }) {
  return (
    <Card variant="outlined" sx={{ height: "100%" }}>
      <CardContent>
        <Stack spacing={1.5}>
          <Typography variant="h6">{nomes[check.name] ?? check.name}</Typography>
          <Chip label={statusLabels[check.status]} color={statusColor(check.status)} sx={{ alignSelf: "flex-start" }} />
          <Typography variant="body2" color="text.secondary">Duração: {check.durationMs} ms</Typography>
        </Stack>
      </CardContent>
    </Card>
  );
}

export default function HealthPage() {
  const [health, setHealth] = useState<HealthDetails | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [atualizando, setAtualizando] = useState(false);
  const [erro, setErro] = useState(false);
  const [ultimaTentativa, setUltimaTentativa] = useState<Date | null>(null);
  const [segundos, setSegundos] = useState(30);
  const emAndamento = useRef(false);
  const possuiHealth = useRef(false);
  const abortController = useRef<AbortController | null>(null);

  const atualizar = useCallback(async () => {
    if (emAndamento.current) return;
    emAndamento.current = true;
    const controller = new AbortController();
    abortController.current = controller;
    setAtualizando(true);
    setUltimaTentativa(new Date());
    setCarregando((atual) => atual && !possuiHealth.current);
    try {
      const resultado = await obterHealthDetalhado(controller.signal);
      setHealth(resultado);
      possuiHealth.current = true;
      setErro(false);
      setSegundos(30);
    } catch {
      setErro(true);
    } finally {
      emAndamento.current = false;
      if (abortController.current === controller) abortController.current = null;
      setAtualizando(false);
      setCarregando(false);
    }
  }, []);

  useEffect(() => {
    void atualizar();
    const polling = window.setInterval(() => void atualizar(), 30000);
    const contador = window.setInterval(() => setSegundos((atual) => Math.max(atual - 1, 0)), 1000);
    return () => {
      window.clearInterval(polling);
      window.clearInterval(contador);
      abortController.current?.abort();
    };
  }, [atualizar]);

  return (
    <Box component="main" sx={{ minHeight: "100vh", bgcolor: "background.default", px: { xs: 2, sm: 4 }, py: { xs: 4, sm: 6 } }}>
      <Container maxWidth="lg">
        <Stack spacing={3}>
          <Stack direction="row" spacing={2} sx={{ alignItems: "center", justifyContent: "space-between" }}>
            <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
              <IconButton component={Link} href="/" aria-label="Voltar para o início"><ArrowBackIcon /></IconButton>
              <Box><Typography variant="h2" component="h1">Saúde do sistema</Typography><Typography color="text.secondary">Diagnóstico técnico da instância atual</Typography></Box>
            </Stack>
            <Button variant="contained" startIcon={<RefreshIcon />} onClick={() => void atualizar()} disabled={atualizando}>Atualizar agora</Button>
          </Stack>

          {carregando && <Stack spacing={2} sx={{ alignItems: "center", py: 8 }}><CircularProgress /><Typography>Consultando health...</Typography></Stack>}
          {erro && <Alert severity="error">Não foi possível consultar a API. Última tentativa: {ultimaTentativa?.toLocaleTimeString("pt-BR") ?? "não realizada"}</Alert>}
          {health && <>
            <Card sx={{ borderLeft: 6, borderColor: `${statusColor(health.status)}.main` }}><CardContent><Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ alignItems: { sm: "center" }, justifyContent: "space-between" }}><Box><Typography variant="overline">Status geral do TaskHub</Typography><Typography variant="h4">{statusLabels[health.status]}</Typography></Box><Chip label={health.status} color={statusColor(health.status)} /></Stack></CardContent></Card>
            <Grid container spacing={2}>{health.checks.map((check) => <Grid key={check.name} size={{ xs: 12, sm: 6 }}><CheckCard check={check} /></Grid>)}<Grid size={{ xs: 12, sm: 6 }}><CheckCard check={{ name: "frontend", status: "Healthy", durationMs: 0 }} /></Grid></Grid>
            <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ color: "text.secondary" }}><Typography>Uptime da API: {formatarUptime(health.uptimeSeconds)}</Typography><Typography>Última verificação: {formatarData(health.checkedAt)}</Typography><Typography>Próxima atualização: {segundos}s</Typography></Stack>
          </>}
        </Stack>
      </Container>
    </Box>
  );
}
