"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import Divider from "@mui/material/Divider";
import IconButton from "@mui/material/IconButton";
import ListItemIcon from "@mui/material/ListItemIcon";
import ListItemText from "@mui/material/ListItemText";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";
import Alert from "@mui/material/Alert";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import VisibilityIcon from "@mui/icons-material/Visibility";

import { excluirTarefa } from "../services/tarefa-service";
import type { Tarefa } from "../types/tarefa";
import { DialogoEditarTarefa } from "./DialogoEditarTarefa";

type MenuAcoesTarefaProps = {
  tarefa: Tarefa;
};

export function MenuAcoesTarefa({ tarefa }: MenuAcoesTarefaProps) {
  const router = useRouter();

  const [ancoraMenu, setAncoraMenu] = useState<null | HTMLElement>(null);

  const [dialogoEdicaoAberto, setDialogoEdicaoAberto] = useState(false);

  const [dialogoExclusaoAberto, setDialogoExclusaoAberto] = useState(false);

  const [excluindo, setExcluindo] = useState(false);

  const [erroExclusao, setErroExclusao] = useState<string | null>(null);

  function abrirMenu(evento: React.MouseEvent<HTMLElement>) {
    setAncoraMenu(evento.currentTarget);
  }

  function fecharMenu() {
    setAncoraMenu(null);
  }

  function abrirDialogoEdicao() {
    fecharMenu();
    setDialogoEdicaoAberto(true);
  }

  function abrirDetalhes() {
    fecharMenu();
    router.push(`/tarefas/${tarefa.id}`);
  }

  function abrirDialogoExclusao() {
    fecharMenu();
    setErroExclusao(null);
    setDialogoExclusaoAberto(true);
  }

  async function confirmarExclusao() {
    if (excluindo) return;

    try {
      setExcluindo(true);
      setErroExclusao(null);

      await excluirTarefa(tarefa.id);

      setDialogoExclusaoAberto(false);
      router.refresh();
    } catch (erro) {
      const mensagem =
        erro instanceof Error
          ? erro.message
          : "Ocorreu um erro desconhecido ao excluir a tarefa.";

      setErroExclusao(mensagem);
    } finally {
      setExcluindo(false);
    }
  }

  return (
    <>
      <Tooltip title="Ações">
        <IconButton
          aria-label={`Abrir ações da tarefa ${tarefa.descricao}`}
          onClick={abrirMenu}
          size="small"
        >
          <MoreVertIcon fontSize="small" />
        </IconButton>
      </Tooltip>

      <Menu
        anchorEl={ancoraMenu}
        open={Boolean(ancoraMenu)}
        onClose={fecharMenu}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
      >
        <MenuItem onClick={abrirDetalhes}>
          <ListItemIcon>
            <VisibilityIcon fontSize="small" />
          </ListItemIcon>

          <ListItemText>Detalhes</ListItemText>
        </MenuItem>

        <MenuItem onClick={abrirDialogoEdicao}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>

          <ListItemText>Editar</ListItemText>
        </MenuItem>

        <Divider />

        <MenuItem onClick={abrirDialogoExclusao} sx={{ color: "error.main" }}>
          <ListItemIcon sx={{ color: "error.main" }}>
            <DeleteIcon fontSize="small" />
          </ListItemIcon>

          <ListItemText>Excluir</ListItemText>
        </MenuItem>
      </Menu>

      <DialogoEditarTarefa
        tarefa={tarefa}
        open={dialogoEdicaoAberto}
        onOpenChange={setDialogoEdicaoAberto}
      />

      <Dialog
        open={dialogoExclusaoAberto}
        onClose={() => {
          if (!excluindo) {
            setDialogoExclusaoAberto(false);
            setErroExclusao(null);
          }
        }}
        aria-labelledby="dialogo-exclusao-titulo"
        aria-describedby="dialogo-exclusao-descricao"
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle id="dialogo-exclusao-titulo" component="h2">
          Excluir tarefa?
        </DialogTitle>

        <DialogContent>
          <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2 }}>
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                width: 44,
                height: 44,
                borderRadius: "50%",
                backgroundColor: "error.light",
                color: "error.dark",
                flexShrink: 0,
              }}
            >
              <WarningAmberIcon />
            </Box>

            <DialogContentText id="dialogo-exclusao-descricao" component="div">
              <Typography component="span" variant="body2">
                A tarefa <strong>“{tarefa.descricao}”</strong> deixará de aparecer
                na listagem ativa.
              </Typography>
            </DialogContentText>
          </Box>

          {erroExclusao && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {erroExclusao}
            </Alert>
          )}
        </DialogContent>

        <DialogActions>
          <Button
            onClick={() => setDialogoExclusaoAberto(false)}
            disabled={excluindo}
            color="inherit"
          >
            Cancelar
          </Button>

          <Button
            onClick={() => {
              void confirmarExclusao();
            }}
            disabled={excluindo}
            color="error"
            variant="contained"
            startIcon={
              excluindo ? (
                <CircularProgress size={16} color="inherit" />
              ) : (
                <DeleteIcon />
              )
            }
          >
            {excluindo ? "Excluindo..." : "Excluir tarefa"}
          </Button>
        </DialogActions>
      </Dialog>

    </>
  );
}
