import Divider from "@mui/material/Divider";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { HistoricoTarefa as HistoricoTarefaType, TipoHistoricoTarefa } from "../types/tarefa";
import { formatarDataHora } from "../utils/formatar-data";

type HistoricoTarefaProps = { historico: HistoricoTarefaType[] };

const rotulosPorTipo: Record<TipoHistoricoTarefa, string> = {
  Criacao: "Tarefa criada",
  AlteracaoDescricao: "Descri\u00e7\u00e3o alterada",
  AlteracaoPrioridade: "Prioridade alterada",
  AlteracaoDataVencimento: "Data de vencimento alterada",
  Conclusao: "Tarefa conclu\u00edda",
  Reabertura: "Tarefa reaberta",
  Exclusao: "Tarefa enviada para a lixeira",
  Restauracao: "Tarefa restaurada",
};

export function HistoricoTarefa({ historico }: HistoricoTarefaProps) {
  if (historico.length === 0) {
    return <Paper variant="outlined" sx={{ p: 3 }}><Typography color="text.secondary">{"Nenhuma altera\u00e7\u00e3o registrada para esta tarefa."}</Typography></Paper>;
  }

  return <Paper variant="outlined"><Stack divider={<Divider flexItem />}>{historico.map((item) => <Stack key={item.id} spacing={0.5} sx={{ p: 2.5 }}><Typography variant="caption" color="text.secondary">{formatarDataHora(item.criadoEm)}</Typography><Typography variant="body1">{rotulosPorTipo[item.tipo]}</Typography>{item.campo && <Stack spacing={0.25} sx={{ mt: 0.5 }}><Typography variant="body2" color="text.secondary">De: {item.valorAnterior ?? "—"}</Typography><Typography variant="body2" color="text.secondary">Para: {item.valorNovo ?? "—"}</Typography></Stack>}</Stack>)}</Stack></Paper>;
}
