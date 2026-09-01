import Chip from "@mui/material/Chip";
import type { SituacaoTarefa } from "../types/tarefa";

type IndicadorSituacaoTarefaProps = {
  situacao: SituacaoTarefa;
};

const configuracaoPorSituacao: Record<
  SituacaoTarefa,
  { color: "warning" | "info" | "success"; label: string }
> = {
  Pendente: { color: "warning", label: "Pendente" },
  "Em andamento": { color: "info", label: "Em andamento" },
  Concluída: { color: "success", label: "Concluída" },
};

export function IndicadorSituacaoTarefa({
  situacao,
}: IndicadorSituacaoTarefaProps) {
  const configuracao = configuracaoPorSituacao[situacao];

  return (
    <Chip label={configuracao.label} color={configuracao.color} size="small" />
  );
}
