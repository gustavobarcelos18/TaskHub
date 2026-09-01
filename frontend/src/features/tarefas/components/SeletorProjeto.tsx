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
  criarProjeto,
  excluirProjeto,
  listarProjetos,
} from "../services/projeto-service";
import type { Projeto } from "../types/tarefa";

type SeletorProjetoProps = { control: Control<TarefaFormData> };

export function SeletorProjeto({ control }: SeletorProjetoProps) {
  const { field } = useController({ control, name: "projetoId" });
  const [opcoes, setOpcoes] = useState<Projeto[]>([]);
  const [novoProjeto, setNovoProjeto] = useState("");
  const [erro, setErro] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [criando, setCriando] = useState(false);
  const [gerenciando, setGerenciando] = useState(false);
  const [projetoParaExcluir, setProjetoParaExcluir] = useState<Projeto | null>(
    null,
  );
  const [excluindo, setExcluindo] = useState(false);

  useEffect(() => {
    let ativo = true;

    listarProjetos()
      .then((projetos) => {
        if (ativo) setOpcoes(projetos);
      })
      .catch((causa: unknown) => {
        if (ativo) {
          setErro(
            causa instanceof Error
              ? causa.message
              : "Não foi possível carregar os projetos.",
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
    const nome = novoProjeto.trim();
    if (!nome || criando) return;

    try {
      setCriando(true);
      setErro(null);
      const projeto = await criarProjeto(nome);
      setOpcoes((itens) =>
        [...itens, projeto].sort((a, b) => a.nome.localeCompare(b.nome)),
      );
      field.onChange(projeto.id);
      setNovoProjeto("");
    } catch (causa) {
      setErro(
        causa instanceof Error
          ? causa.message
          : "Não foi possível criar o projeto.",
      );
    } finally {
      setCriando(false);
    }
  }

  async function excluir() {
    if (!projetoParaExcluir || excluindo) return;

    try {
      setExcluindo(true);
      setErro(null);
      await excluirProjeto(projetoParaExcluir.id);
      setOpcoes((itens) =>
        itens.filter((item) => item.id !== projetoParaExcluir.id),
      );
      if (field.value === projetoParaExcluir.id) field.onChange(null);
      setProjetoParaExcluir(null);
    } catch (causa) {
      setErro(
        causa instanceof Error
          ? causa.message
          : "Não foi possível excluir o projeto.",
      );
    } finally {
      setExcluindo(false);
    }
  }

  return (
    <>
      <Stack spacing={1}>
        <Autocomplete
          loading={carregando}
          options={opcoes}
          value={opcoes.find((opcao) => opcao.id === field.value) ?? null}
          onChange={(_, valor) => field.onChange(valor?.id ?? null)}
          getOptionLabel={(opcao) => opcao.nome}
          isOptionEqualToValue={(opcao, valor) => opcao.id === valor.id}
          renderInput={(params) => (
            <TextField
              {...params}
              label="Projeto"
              helperText="Opcional. Selecione um projeto ou deixe sem projeto."
            />
          )}
        />

        <Stack direction={{ xs: "column", sm: "row" }} spacing={1}>
          <TextField
            label="Novo projeto"
            value={novoProjeto}
            onChange={(evento) => setNovoProjeto(evento.target.value)}
            slotProps={{ htmlInput: { maxLength: 100 } }}
          />
          <Button
            variant="outlined"
            disabled={!novoProjeto.trim() || criando}
            startIcon={criando ? <CircularProgress size={16} /> : undefined}
            onClick={() => void criar()}
          >
            Criar projeto
          </Button>
          <Button variant="text" onClick={() => setGerenciando(true)}>
            Gerenciar projetos
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
        <DialogTitle>Gerenciar projetos</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            Excluir um projeto não exclui as tarefas. Elas ficarão sem projeto.
          </DialogContentText>
          <Stack spacing={1}>
            {opcoes.length === 0 && (
              <Typography color="text.secondary">
                Nenhum projeto cadastrado.
              </Typography>
            )}
            {opcoes.map((projeto) => (
              <Stack
                key={projeto.id}
                direction="row"
                spacing={1}
                sx={{ justifyContent: "space-between", alignItems: "center" }}
              >
                <Typography>{projeto.nome}</Typography>
                <Button
                  color="error"
                  disabled={excluindo}
                  onClick={() => setProjetoParaExcluir(projeto)}
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
        open={projetoParaExcluir !== null}
        onClose={() => {
          if (!excluindo) setProjetoParaExcluir(null);
        }}
      >
        <DialogTitle>Excluir projeto?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            O projeto “{projetoParaExcluir?.nome}” será excluído. As tarefas
            vinculadas permanecerão cadastradas e ficarão sem projeto.
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
            onClick={() => setProjetoParaExcluir(null)}
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
            Excluir projeto
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
