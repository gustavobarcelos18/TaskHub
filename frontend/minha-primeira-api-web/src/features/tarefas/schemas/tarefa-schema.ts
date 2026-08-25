import { z } from "zod";

export const LIMITE_DESCRICAO_TAREFA = 50;

export const tarefaSchema = z.object({
  descricao: z
    .string()
    .trim()
    .min(1, "A descrição é obrigatória.")
    .max(
      LIMITE_DESCRICAO_TAREFA,
      `A descrição deve ter no máximo ${LIMITE_DESCRICAO_TAREFA} caracteres.`,
    ),

  situacao: z.enum(["Pendente", "Em andamento", "Concluída"], {
    message: "Selecione uma situação válida.",
  }),
});

export type TarefaFormData = z.infer<typeof tarefaSchema>;
