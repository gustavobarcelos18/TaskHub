import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import Divider from "@mui/material/Divider";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { Tarefa } from "../types/tarefa";
import { formatarDataCivil, formatarDataHora } from "../utils/formatar-data";
import { IndicadorPrioridadeTarefa } from "./IndicadorPrioridadeTarefa";
import { IndicadorSituacaoTarefa } from "./IndicadorSituacaoTarefa";

type DetalhesTarefaConteudoProps = { tarefa: Tarefa };
type CampoProps = { titulo: string; valor: React.ReactNode };

function Campo({ titulo, valor }: CampoProps) {
  return (
    <Stack spacing={0.5}>
      <Typography variant="caption" color="text.secondary">
        {titulo}
      </Typography>
      <Typography variant="body1" component="div">
        {valor}
      </Typography>
    </Stack>
  );
}

export function DetalhesTarefaConteudo({
  tarefa,
}: DetalhesTarefaConteudoProps) {
  return (
    <Stack spacing={3}>
      <Campo titulo="Descrição" valor={tarefa.descricao} />
      <Divider />
      <Campo titulo="Projeto" valor={tarefa.projeto?.nome ?? "Sem projeto."} />
      <Divider />
      <Campo
        titulo="Etiquetas"
        valor={
          tarefa.etiquetas.length ? (
            <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
              {tarefa.etiquetas.map((etiqueta) => (
                <Chip key={etiqueta.id} label={etiqueta.nome} size="small" />
              ))}
            </Box>
          ) : (
            "Sem etiquetas."
          )
        }
      />
      <Divider />
      <Campo
        titulo="Observações"
        valor={
          tarefa.observacoes ? (
            <Box sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}>
              {tarefa.observacoes}
            </Box>
          ) : (
            "Sem observações."
          )
        }
      />
      <Divider />
      <Campo
        titulo="Situação"
        valor={<IndicadorSituacaoTarefa situacao={tarefa.situacao} />}
      />
      <Divider />
      <Campo
        titulo="Prioridade"
        valor={<IndicadorPrioridadeTarefa prioridade={tarefa.prioridade} />}
      />
      <Divider />
      <Campo
        titulo="Data de vencimento"
        valor={formatarDataCivil(tarefa.dataVencimento)}
      />
      <Divider />
      <Campo titulo="Criada em" valor={formatarDataHora(tarefa.criadaEm)} />
      <Campo
        titulo="Modificada em"
        valor={formatarDataHora(tarefa.modificadaEm)}
      />
      <Campo
        titulo="Finalizada em"
        valor={formatarDataHora(tarefa.concluidaEm)}
      />
    </Stack>
  );
}
