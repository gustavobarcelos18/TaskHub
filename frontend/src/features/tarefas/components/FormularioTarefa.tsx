"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm, Controller } from "react-hook-form";
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
  LIMITE_OBSERVACOES_TAREFA,
  tarefaSchema,
  type TarefaFormData,
} from "../schemas/tarefa-schema";

import { criarTarefa } from "../services/tarefa-service";
import { PRIORIDADES_TAREFA, SITUACOES_TAREFA } from "../types/tarefa";
import { mascararDataCivil } from "../utils/formatar-data";
import { SeletorEtiquetas } from "./SeletorEtiquetas";
import { SeletorProjeto } from "./SeletorProjeto";

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
      observacoes: "",
      situacao: "Pendente",
      prioridade: "Media",
      dataVencimento: "",
      projetoId: null,
      etiquetaIds: [],
    },
  });

  async function cadastrarTarefa(dados: TarefaFormData) {
    try {
      setErroCadastro(null);

      await criarTarefa(dados);

      router.push("/tarefas?feedback=tarefaCriada");
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

        <Controller
          name="prioridade"
          control={control}
          render={({ field }) => (
            <TextField
              id="prioridade"
              label="Prioridade"
              select
              value={field.value}
              onChange={field.onChange}
              onBlur={field.onBlur}
              error={Boolean(errors.prioridade)}
              helperText={errors.prioridade?.message}
              fullWidth
            >
              {PRIORIDADES_TAREFA.map((prioridade) => (
                <MenuItem key={prioridade} value={prioridade}>
                  {prioridade === "Media" ? "Média" : prioridade}
                </MenuItem>
              ))}
            </TextField>
          )}
        />

        <TextField
          id="observacoes"
          label="Observações"
          placeholder="Adicione detalhes complementares da tarefa"
          multiline
          minRows={4}
          slotProps={{ htmlInput: { maxLength: LIMITE_OBSERVACOES_TAREFA } }}
          error={Boolean(errors.observacoes)}
          helperText={
            errors.observacoes?.message ??
            `Opcional. Máximo de ${LIMITE_OBSERVACOES_TAREFA} caracteres.`
          }
          fullWidth
          {...register("observacoes")}
        />

        <SeletorEtiquetas control={control} />

        <SeletorProjeto control={control} />

        <Controller
          name="dataVencimento"
          control={control}
          render={({ field }) => (
            <TextField
              id="dataVencimento"
              label="Data de vencimento"
              placeholder="dd/mm/aaaa"
              value={field.value}
              onChange={(evento) =>
                field.onChange(mascararDataCivil(evento.target.value))
              }
              onBlur={field.onBlur}
              slotProps={{ htmlInput: { inputMode: "numeric", maxLength: 10 } }}
              error={Boolean(errors.dataVencimento)}
              helperText={errors.dataVencimento?.message ?? "Opcional"}
              fullWidth
            />
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
