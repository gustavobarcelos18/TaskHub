import Chip from "@mui/material/Chip";
import type { PrioridadeTarefa } from "../types/tarefa";

type IndicadorPrioridadeTarefaProps = { prioridade: PrioridadeTarefa };

const configuracaoPorPrioridade: Record<PrioridadeTarefa, { color: "default" | "warning" | "error"; label: string }> = {
  Baixa: { color: "default", label: "Baixa" },
  Media: { color: "warning", label: "Média" },
  Alta: { color: "error", label: "Alta" },
};

export function IndicadorPrioridadeTarefa({ prioridade }: IndicadorPrioridadeTarefaProps) {
  const configuracao = configuracaoPorPrioridade[prioridade];
  return <Chip label={configuracao.label} color={configuracao.color} size="small" />;
}
