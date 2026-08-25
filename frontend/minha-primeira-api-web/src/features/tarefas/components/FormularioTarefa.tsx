"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  useForm,
  Controller,
} from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";

import {
  LIMITE_DESCRICAO_TAREFA,
  tarefaSchema,
  type TarefaFormData,
} from "../schemas/tarefa-schema";

import { criarTarefa } from "../services/tarefa-service";
import { SITUACOES_TAREFA } from "../types/tarefa";

export function FormularioTarefa() {
  const router = useRouter();

  const [erroCadastro, setErroCadastro] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<TarefaFormData>({
    resolver: zodResolver(tarefaSchema),

    defaultValues: {
      descricao: "",
      situacao: "Pendente",
    },
  });

  async function cadastrarTarefa(dados: TarefaFormData) {
    try {
      setErroCadastro(null);

      await criarTarefa(dados);

      router.push("/tarefas");
    } catch (erro) {
      const mensagem =
        erro instanceof Error
          ? erro.message
          : "Ocorreu um erro desconhecido ao cadastrar a tarefa.";

      setErroCadastro(mensagem);
    }
  }

  return (
    <Paper
      component="form"
      onSubmit={handleSubmit(cadastrarTarefa)}
      noValidate
      variant="outlined"
      sx={{ p: 3 }}
    >
      <Stack spacing={3}>
        <Box>
          <TextField
            id="descricao"
            label="Descrição"
            placeholder="Digite a descrição da tarefa"
            slotProps={{ htmlInput: { maxLength: LIMITE_DESCRICAO_TAREFA } }}
            error={Boolean(errors.descricao)}
            helperText={errors.descricao?.message}
            fullWidth
            {...register("descricao")}
          />

          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ display: "block", mt: 0.5 }}
          >
            Máximo de {LIMITE_DESCRICAO_TAREFA} caracteres.
          </Typography>
        </Box>

        <Controller
          name="situacao"
          control={control}
          render={({ field }) => (
            <TextField
              id="situacao"
              label="Situação"
              select
              value={field.value}
              onChange={field.onChange}
              onBlur={field.onBlur}
              error={Boolean(errors.situacao)}
              helperText={errors.situacao?.message}
              fullWidth
            >
              {SITUACOES_TAREFA.map((situacao) => (
                <MenuItem key={situacao} value={situacao}>
                  {situacao}
                </MenuItem>
              ))}
            </TextField>
          )}
        />

        {erroCadastro && <Alert severity="error">{erroCadastro}</Alert>}

        <Box sx={{ display: "flex", justifyContent: "flex-end" }}>
          <Button
            type="submit"
            variant="contained"
            disabled={isSubmitting}
            startIcon={
              isSubmitting ? (
                <CircularProgress size={16} color="inherit" />
              ) : undefined
            }
          >
            {isSubmitting ? "Cadastrando..." : "Cadastrar tarefa"}
          </Button>
        </Box>
      </Stack>
    </Paper>
  );
}
