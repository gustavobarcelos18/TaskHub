"use client";

import { useEffect, useState } from "react";
import { useController, type Control } from "react-hook-form";
import {
  Alert,
  Autocomplete,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import type { TarefaFormData } from "../schemas/tarefa-schema";
import {
  criarEtiqueta,
  excluirEtiqueta,
  listarEtiquetas,
} from "../services/etiqueta-service";
import type { Etiqueta } from "../types/tarefa";

type SeletorEtiquetasProps = { control: Control<TarefaFormData> };

export function SeletorEtiquetas({ control }: SeletorEtiquetasProps) {
  const { field } = useController({ control, name: "etiquetaIds" });
  const [opcoes, setOpcoes] = useState<Etiqueta[]>([]);
  const [novaEtiqueta, setNovaEtiqueta] = useState("");
  const [erro, setErro] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [criando, setCriando] = useState(false);
  const [gerenciando, setGerenciando] = useState(false);
  const [etiquetaParaExcluir, setEtiquetaParaExcluir] =
    useState<Etiqueta | null>(null);
  const [excluindo, setExcluindo] = useState(false);

  useEffect(() => {
    let ativo = true;

    listarEtiquetas()
      .then((etiquetas) => {
        if (ativo) setOpcoes(etiquetas);
      })
      .catch((causa: unknown) => {
        if (ativo) {
          setErro(
            causa instanceof Error
              ? causa.message
              : "Não foi possível carregar as etiquetas.",
          );
        }
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, []);

  async function criar() {
    const nome = novaEtiqueta.trim();
    if (!nome || criando) return;

    try {
      setCriando(true);
      setErro(null);
      const etiqueta = await criarEtiqueta(nome);
      setOpcoes((itens) =>
        [...itens, etiqueta].sort((a, b) => a.nome.localeCompare(b.nome)),
      );
      field.onChange([...new Set([...field.value, etiqueta.id])]);
      setNovaEtiqueta("");
    } catch (causa) {
      setErro(
        causa instanceof Error
          ? causa.message
          : "Não foi possível criar a etiqueta.",
      );
    } finally {
      setCriando(false);
    }
  }

  async function excluir() {
    if (!etiquetaParaExcluir || excluindo) return;

    try {
      setExcluindo(true);
      setErro(null);
      await excluirEtiqueta(etiquetaParaExcluir.id);
      setOpcoes((itens) =>
        itens.filter((item) => item.id !== etiquetaParaExcluir.id),
      );
      field.onChange(field.value.filter((id) => id !== etiquetaParaExcluir.id));
      setEtiquetaParaExcluir(null);
    } catch (causa) {
      setErro(
        causa instanceof Error
          ? causa.message
          : "Não foi possível excluir a etiqueta.",
      );
    } finally {
      setExcluindo(false);
    }
  }

  return (
    <>
      <Stack spacing={1}>
        <Autocomplete
          multiple
          loading={carregando}
          options={opcoes}
          value={opcoes.filter((opcao) => field.value.includes(opcao.id))}
          onChange={(_, valores) =>
            field.onChange(valores.map((valor) => valor.id))
          }
          getOptionLabel={(opcao) => opcao.nome}
          isOptionEqualToValue={(opcao, valor) => opcao.id === valor.id}
          renderInput={(params) => (
            <TextField
              {...params}
              label="Etiquetas"
              helperText="Opcional. Use as etiquetas existentes ou crie uma nova."
            />
          )}
        />

        <Stack direction={{ xs: "column", sm: "row" }} spacing={1}>
          <TextField
            label="Nova etiqueta"
            value={novaEtiqueta}
            onChange={(evento) => setNovaEtiqueta(evento.target.value)}
            slotProps={{ htmlInput: { maxLength: 50 } }}
          />
          <Button
            variant="outlined"
            disabled={!novaEtiqueta.trim() || criando}
            startIcon={criando ? <CircularProgress size={16} /> : undefined}
            onClick={() => void criar()}
          >
            Criar etiqueta
          </Button>
          <Button variant="text" onClick={() => setGerenciando(true)}>
            Gerenciar etiquetas
          </Button>
        </Stack>

        {erro && <Alert severity="error">{erro}</Alert>}
      </Stack>

      <Dialog
        open={gerenciando}
        onClose={() => {
          if (!excluindo) setGerenciando(false);
        }}
        fullWidth
        maxWidth="xs"
      >
        <DialogTitle>Gerenciar etiquetas</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            Excluir uma etiqueta a remove de todas as tarefas, mas não exclui as
            tarefas.
          </DialogContentText>
          <Stack spacing={1}>
            {opcoes.length === 0 && (
              <Typography color="text.secondary">
                Nenhuma etiqueta cadastrada.
              </Typography>
            )}
            {opcoes.map((etiqueta) => (
              <Stack
                key={etiqueta.id}
                direction="row"
                spacing={1}
                sx={{ justifyContent: "space-between", alignItems: "center" }}
              >
                <Typography>{etiqueta.nome}</Typography>
                <Button
                  color="error"
                  disabled={excluindo}
                  onClick={() => setEtiquetaParaExcluir(etiqueta)}
                >
                  Excluir
                </Button>
              </Stack>
            ))}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button disabled={excluindo} onClick={() => setGerenciando(false)}>
            Fechar
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={etiquetaParaExcluir !== null}
        onClose={() => {
          if (!excluindo) setEtiquetaParaExcluir(null);
        }}
      >
        <DialogTitle>Excluir etiqueta?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            A etiqueta “{etiquetaParaExcluir?.nome}” será removida de todas as
            tarefas. As tarefas não serão excluídas.
          </DialogContentText>
          {erro && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {erro}
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button
            disabled={excluindo}
            onClick={() => setEtiquetaParaExcluir(null)}
          >
            Cancelar
          </Button>
          <Button
            color="error"
            variant="contained"
            disabled={excluindo}
            startIcon={
              excluindo ? (
                <CircularProgress size={16} color="inherit" />
              ) : undefined
            }
            onClick={() => void excluir()}
          >
            Excluir etiqueta
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
