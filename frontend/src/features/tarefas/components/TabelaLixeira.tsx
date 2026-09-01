"use client";

import { useState } from "react";
import Alert from "@mui/material/Alert";
import Snackbar from "@mui/material/Snackbar";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import DeleteForeverIcon from "@mui/icons-material/DeleteForever";
import RestoreIcon from "@mui/icons-material/Restore";
import type { Tarefa } from "../types/tarefa";
import {
  excluirTarefaPermanentemente,
  restaurarTarefa,
} from "../services/tarefa-service";
import { formatarDataHora } from "../utils/formatar-data";
import { IndicadorSituacaoTarefa } from "./IndicadorSituacaoTarefa";

type TabelaLixeiraProps = { tarefas: Tarefa[] };

export function TabelaLixeira({
  tarefas: tarefasIniciais,
}: TabelaLixeiraProps) {
  const [tarefas, setTarefas] = useState(tarefasIniciais);
  const [tarefaSelecionada, setTarefaSelecionada] = useState<Tarefa | null>(
    null,
  );
  const [processandoId, setProcessandoId] = useState<number | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);

  function removerDaLista(tarefaId: number) {
    setTarefas((tarefasAtuais) =>
      tarefasAtuais.filter((tarefa) => tarefa.id !== tarefaId),
    );
  }

  async function restaurar(tarefa: Tarefa) {
    try {
      setProcessandoId(tarefa.id);
      setErro(null);
      await restaurarTarefa(tarefa.id);
      removerDaLista(tarefa.id);
      setFeedback("Tarefa restaurada com sucesso.");
    } catch (erro) {
      setErro(
        erro instanceof Error
          ? erro.message
          : "Não foi possível restaurar a tarefa.",
      );
    } finally {
      setProcessandoId(null);
    }
  }

  async function confirmarExclusaoPermanente() {
    if (!tarefaSelecionada) return;

    try {
      setProcessandoId(tarefaSelecionada.id);
      setErro(null);
      await excluirTarefaPermanentemente(tarefaSelecionada.id);
      removerDaLista(tarefaSelecionada.id);
      setTarefaSelecionada(null);
      setFeedback("Tarefa excluída permanentemente.");
    } catch (erro) {
      setErro(
        erro instanceof Error
          ? erro.message
          : "Não foi possível excluir permanentemente a tarefa.",
      );
    } finally {
      setProcessandoId(null);
    }
  }

  if (tarefas.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 6 }}>
        <Typography align="center" color="text.secondary">
          Nenhuma tarefa na lixeira.
        </Typography>
      </Paper>
    );
  }

  return (
    <>
      <Stack spacing={2}>
        {erro && <Alert severity="error">{erro}</Alert>}

        <TableContainer component={Paper} variant="outlined">
          <Box sx={{ overflowX: "auto" }}>
            <Table
              sx={{ minWidth: 800 }}
              aria-label="Tabela de tarefas excluídas"
            >
              <TableHead>
                <TableRow>
                  <TableCell>Descrição</TableCell>
                  <TableCell width={180}>Situação</TableCell>
                  <TableCell width={200}>Excluída em</TableCell>
                  <TableCell width={250} align="right">
                    Ações
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {tarefas.map((tarefa) => (
                  <TableRow key={tarefa.id} hover>
                    <TableCell component="th" scope="row">
                      {tarefa.descricao}
                    </TableCell>
                    <TableCell>
                      <IndicadorSituacaoTarefa situacao={tarefa.situacao} />
                    </TableCell>
                    <TableCell>{formatarDataHora(tarefa.excluidaEm)}</TableCell>
                    <TableCell align="right">
                      <Stack
                        direction="row"
                        spacing={1}
                        sx={{ justifyContent: "flex-end" }}
                      >
                        <Button
                          size="small"
                          startIcon={
                            processandoId === tarefa.id ? (
                              <CircularProgress size={16} />
                            ) : (
                              <RestoreIcon />
                            )
                          }
                          disabled={processandoId !== null}
                          onClick={() => {
                            void restaurar(tarefa);
                          }}
                        >
                          Restaurar
                        </Button>
                        <Button
                          size="small"
                          color="error"
                          startIcon={<DeleteForeverIcon />}
                          disabled={processandoId !== null}
                          onClick={() => {
                            setErro(null);
                            setTarefaSelecionada(tarefa);
                          }}
                        >
                          Excluir definitivamente
                        </Button>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Box>
        </TableContainer>
      </Stack>

      <Dialog
        open={tarefaSelecionada !== null}
        onClose={() => {
          if (processandoId === null) setTarefaSelecionada(null);
        }}
      >
        <DialogTitle>Excluir permanentemente?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            A tarefa “{tarefaSelecionada?.descricao}” será removida
            permanentemente e esta ação não poderá ser desfeita.
          </DialogContentText>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            Todo o histórico desta tarefa também será removido. Não será
            possível restaurá-la.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button
            disabled={processandoId !== null}
            onClick={() => setTarefaSelecionada(null)}
          >
            Cancelar
          </Button>
          <Button
            color="error"
            variant="contained"
            disabled={processandoId !== null}
            startIcon={
              processandoId !== null ? (
                <CircularProgress size={16} color="inherit" />
              ) : (
                <DeleteForeverIcon />
              )
            }
            onClick={() => {
              void confirmarExclusaoPermanente();
            }}
          >
            Excluir permanentemente
          </Button>
        </DialogActions>
      </Dialog>
      <Snackbar
        open={feedback !== null}
        autoHideDuration={4000}
        onClose={() => setFeedback(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          severity="success"
          variant="filled"
          onClose={() => setFeedback(null)}
        >
          {feedback}
        </Alert>
      </Snackbar>
    </>
  );
}
