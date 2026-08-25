import Box from "@mui/material/Box";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { HistoricoTarefa as HistoricoTarefaType, TipoHistoricoTarefa } from "../types/tarefa";
import { formatarDataCivil, formatarDataHora } from "../utils/formatar-data";

type HistoricoTarefaProps = { historico: HistoricoTarefaType[] };

const rotulosPorTipo: Record<TipoHistoricoTarefa, string> = {
  Criacao: "Tarefa criada",
  AlteracaoDescricao: "Descrição alterada",
  AlteracaoObservacoes: "Observações alteradas",
  AlteracaoEtiquetas: "Etiquetas alteradas",
  AlteracaoPrioridade: "Prioridade alterada",
  AlteracaoDataVencimento: "Data de vencimento alterada",
  AlteracaoSituacao: "Situação alterada",
  Conclusao: "Tarefa concluída",
  Reabertura: "Tarefa reaberta",
  Exclusao: "Tarefa enviada para a lixeira",
  Restauracao: "Tarefa restaurada",
};

function formatarValor(item: HistoricoTarefaType, valor: string | null): string {
  if (item.tipo === "AlteracaoEtiquetas") {
    try { const nomes: unknown = valor ? JSON.parse(valor) : []; return Array.isArray(nomes) && nomes.every((nome) => typeof nome === "string") ? nomes.length ? nomes.join(", ") : "Sem etiquetas" : "—"; } catch { return "—"; }
  }
  if (item.tipo === "AlteracaoPrioridade" && valor === "Media") return "Média";
  if (item.tipo === "AlteracaoDataVencimento") return formatarDataCivil(valor);
  return valor ?? (item.tipo === "AlteracaoObservacoes" ? "Sem observações" : "—");
}

function ValoresAlteracao({ item }: { item: HistoricoTarefaType }) {
  if (item.tipo !== "AlteracaoObservacoes") return <Typography variant="body2" color="text.secondary">{formatarValor(item, item.valorAnterior)} → {formatarValor(item, item.valorNovo)}</Typography>;

  const sxTexto = { whiteSpace: "pre-wrap", overflowWrap: "anywhere", display: "-webkit-box", WebkitLineClamp: 4, WebkitBoxOrient: "vertical", overflow: "hidden" };

  return <Stack spacing={0.25}><Typography variant="caption" color="text.secondary">De:</Typography><Typography variant="body2" sx={sxTexto}>{formatarValor(item, item.valorAnterior)}</Typography><Typography variant="caption" color="text.secondary">Para:</Typography><Typography variant="body2" sx={sxTexto}>{formatarValor(item, item.valorNovo)}</Typography></Stack>;
}

export function HistoricoTarefa({ historico }: HistoricoTarefaProps) {
  if (historico.length === 0) return <Paper variant="outlined" sx={{ p: 3 }}><Typography color="text.secondary">Nenhum histórico registrado.</Typography></Paper>;

  return <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}><Stack spacing={0}>{historico.map((item, indice) => <Stack key={item.id} direction="row" spacing={2} sx={{ minHeight: 72 }}><Stack sx={{ alignItems: "center", width: 18, flexShrink: 0 }}><Box aria-hidden="true" sx={{ width: 10, height: 10, mt: 0.75, borderRadius: "50%", bgcolor: "primary.main" }} />{indice < historico.length - 1 && <Box aria-hidden="true" sx={{ width: 2, flexGrow: 1, my: 0.5, bgcolor: "divider" }} />}</Stack><Stack spacing={0.5} sx={{ pb: 2, minWidth: 0 }}><Typography variant="caption" color="text.secondary">{formatarDataHora(item.criadoEm)}</Typography><Typography variant="body1">{rotulosPorTipo[item.tipo]}</Typography>{item.campo && <ValoresAlteracao item={item} />}</Stack></Stack>)}</Stack></Paper>;
}
