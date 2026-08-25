"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, Controller } from "react-hook-form";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import CloseIcon from "@mui/icons-material/Close";

import {
  LIMITE_DESCRICAO_TAREFA,
  LIMITE_OBSERVACOES_TAREFA,
  tarefaSchema,
  type TarefaFormData,
} from "../schemas/tarefa-schema";

import { atualizarTarefa } from "../services/tarefa-service";
import { PRIORIDADES_TAREFA, SITUACOES_TAREFA, type Tarefa } from "../types/tarefa";
import { converterDataParaFormulario, mascararDataCivil } from "../utils/formatar-data";
import { SeletorEtiquetas } from "./SeletorEtiquetas";

type DialogoEditarTarefaProps = {
  tarefa: Tarefa;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: (mensagem: string) => void;
};

export function DialogoEditarTarefa({
  tarefa,
  open,
  onOpenChange,
  onSuccess,
}: DialogoEditarTarefaProps) {
  const router = useRouter();

  const [erroEdicao, setErroEdicao] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<TarefaFormData>({
    resolver: zodResolver(tarefaSchema),

    defaultValues: {
      descricao: tarefa.descricao,
      observacoes: tarefa.observacoes ?? "",
      situacao: tarefa.situacao,
      prioridade: tarefa.prioridade,
      dataVencimento: converterDataParaFormulario(tarefa.dataVencimento),
      etiquetaIds: tarefa.etiquetas.map((etiqueta) => etiqueta.id),
    },
  });

  async function salvarAlteracoes(dados: TarefaFormData) {
    try {
      setErroEdicao(null);

      await atualizarTarefa(tarefa.id, dados);

      onOpenChange(false);
      onSuccess(dados.situacao === "Concluída" && tarefa.situacao !== "Concluída" ? "Tarefa concluída com sucesso." : tarefa.situacao === "Concluída" && dados.situacao !== "Concluída" ? "Tarefa reaberta com sucesso." : "Tarefa atualizada com sucesso.");
      router.refresh();
    } catch (erro) {
      const mensagem =
        erro instanceof Error
          ? erro.message
          : "Ocorreu um erro desconhecido ao atualizar a tarefa.";

      setErroEdicao(mensagem);
    }
  }

  return (
    <Dialog
      open={open}
      onClose={() => {
        if (!isSubmitting) {
          onOpenChange(false);
        }
      }}
      aria-labelledby="dialogo-edicao-titulo"
      aria-describedby="dialogo-edicao-descricao"
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle id="dialogo-edicao-titulo" component="h2">
        Editar tarefa

        <IconButton
          aria-label="Fechar janela de edição"
          onClick={() => onOpenChange(false)}
          disabled={isSubmitting}
          sx={{
            position: "absolute",
            right: 8,
            top: 8,
          }}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent>
        <Typography
          id="dialogo-edicao-descricao"
          variant="body2"
          color="text.secondary"
          sx={{ mb: 3 }}
        >
          Altere a descrição ou a situação da tarefa.
        </Typography>

        <Box
          component="form"
          onSubmit={handleSubmit(salvarAlteracoes)}
          noValidate
          sx={{ display: "flex", flexDirection: "column", gap: 3 }}
        >
          <TextField
            id={`descricao-${tarefa.id}`}
            label="Descrição"
            placeholder="Digite a descrição da tarefa"
            autoFocus
            slotProps={{ htmlInput: { maxLength: LIMITE_DESCRICAO_TAREFA } }}
            error={Boolean(errors.descricao)}
            helperText={errors.descricao?.message}
            fullWidth
            {...register("descricao")}
          />

          <TextField
            id={`observacoes-${tarefa.id}`}
            label="Observações"
            placeholder="Adicione detalhes complementares da tarefa"
            multiline
            minRows={4}
            slotProps={{ htmlInput: { maxLength: LIMITE_OBSERVACOES_TAREFA } }}
            error={Boolean(errors.observacoes)}
            helperText={errors.observacoes?.message ?? `Opcional. Máximo de ${LIMITE_OBSERVACOES_TAREFA} caracteres.`}
            fullWidth
            {...register("observacoes")}
          />

          <SeletorEtiquetas control={control} />

          <Controller
            name="prioridade"
            control={control}
            render={({ field }) => (
              <TextField id={`prioridade-${tarefa.id}`} label="Prioridade" select value={field.value} onChange={field.onChange} onBlur={field.onBlur} error={Boolean(errors.prioridade)} helperText={errors.prioridade?.message} fullWidth>
                {PRIORIDADES_TAREFA.map((prioridade) => <MenuItem key={prioridade} value={prioridade}>{prioridade === "Media" ? "Média" : prioridade}</MenuItem>)}
              </TextField>
            )}
          />

          <Controller
            name="dataVencimento"
            control={control}
            render={({ field }) => (
              <TextField
                id={`dataVencimento-${tarefa.id}`}
                label="Data de vencimento"
                placeholder="dd/mm/aaaa"
                value={field.value}
                onChange={(evento) => field.onChange(mascararDataCivil(evento.target.value))}
                onBlur={field.onBlur}
                slotProps={{ htmlInput: { inputMode: "numeric", maxLength: 10 } }}
                error={Boolean(errors.dataVencimento)}
                helperText={errors.dataVencimento?.message ?? "Opcional"}
                fullWidth
              />
            )}
          />

          <Controller
            name="situacao"
            control={control}
            render={({ field }) => (
              <TextField
                id={`situacao-${tarefa.id}`}
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

          {erroEdicao && <Alert severity="error">{erroEdicao}</Alert>}

          <DialogActions sx={{ px: 0, pb: 0 }}>
            <Button
              onClick={() => onOpenChange(false)}
              disabled={isSubmitting}
              color="inherit"
            >
              Cancelar
            </Button>

            <Button
              type="submit"
              disabled={isSubmitting}
              variant="contained"
              startIcon={
                isSubmitting ? (
                  <CircularProgress size={16} color="inherit" />
                ) : undefined
              }
            >
              {isSubmitting ? "Salvando..." : "Salvar alterações"}
            </Button>
          </DialogActions>
        </Box>
      </DialogContent>
    </Dialog>
  );
}
