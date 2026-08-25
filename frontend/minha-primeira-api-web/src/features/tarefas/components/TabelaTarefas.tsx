import Box from "@mui/material/Box";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import AddIcon from "@mui/icons-material/Add";
import { BotaoLink } from "@/components/ComponentesRoteador";

import type { Tarefa } from "../types/tarefa";
import { formatarDataHora } from "../utils/formatar-data";
import { IndicadorSituacaoTarefa } from "./IndicadorSituacaoTarefa";
import { MenuAcoesTarefa } from "./MenuAcoesTarefa";

type TabelaTarefasProps = {
  tarefas: Tarefa[];
};

export function TabelaTarefas({ tarefas }: TabelaTarefasProps) {
  if (tarefas.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 6 }}>
        <Stack spacing={2} sx={{ alignItems: "center" }}>
          <Typography variant="body1" color="text.secondary">
            Nenhuma tarefa cadastrada.
          </Typography>

          <BotaoLink
            href="/tarefas/criar"
            variant="contained"
            startIcon={<AddIcon />}
          >
            Criar primeira tarefa
          </BotaoLink>
        </Stack>
      </Paper>
    );
  }

  return (
    <TableContainer component={Paper} variant="outlined">
      <Box sx={{ overflowX: "auto" }}>
        <Table sx={{ minWidth: 800 }} aria-label="Tabela de tarefas">
          <TableHead>
            <TableRow>
              <TableCell>Descrição</TableCell>
              <TableCell width={180}>Situação</TableCell>
              <TableCell width={220}>Última atualização</TableCell>
              <TableCell width={90} align="right">
                Ações
              </TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {tarefas.map((tarefa) => {
              const ultimaAtualizacao = tarefa.modificadaEm ?? tarefa.criadaEm;

              const tipoAtualizacao = tarefa.modificadaEm
                ? "Modificação"
                : "Criação";

              return (
                <TableRow
                  key={tarefa.id}
                  hover
                  sx={{ "&:hover": { backgroundColor: "action.hover" } }}
                >
                  <TableCell
                    component="th"
                    scope="row"
                    title={tarefa.descricao}
                    sx={{ whiteSpace: "normal", wordBreak: "break-word" }}
                  >
                    {tarefa.descricao}
                  </TableCell>

                  <TableCell>
                    <IndicadorSituacaoTarefa situacao={tarefa.situacao} />
                  </TableCell>

                  <TableCell>
                    <Typography variant="body2" color="text.primary">
                      {formatarDataHora(ultimaAtualizacao)}
                    </Typography>

                    <Typography variant="caption" color="text.secondary">
                      {tipoAtualizacao}
                    </Typography>
                  </TableCell>

                  <TableCell align="right">
                    <MenuAcoesTarefa tarefa={tarefa} />
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </Box>
    </TableContainer>
  );
}