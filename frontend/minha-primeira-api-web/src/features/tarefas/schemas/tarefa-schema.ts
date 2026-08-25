import { z } from "zod";

import { SITUACOES_TAREFA } from "../types/tarefa";

export const LIMITE_DESCRICAO_TAREFA = 200;

export const tarefaSchema = z.object({
  descricao: z
    .string()
    .trim()
    .min(1, "A descrição é obrigatória.")
    .max(
      LIMITE_DESCRICAO_TAREFA,
      `A descrição deve ter no máximo ${LIMITE_DESCRICAO_TAREFA} caracteres.`,
    ),

  situacao: z.enum(SITUACOES_TAREFA, {
    message: "Selecione uma situação válida.",
  }),
});

export type TarefaFormData = z.infer<typeof tarefaSchema>;
