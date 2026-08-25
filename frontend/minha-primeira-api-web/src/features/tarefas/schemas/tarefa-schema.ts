import { z } from "zod";

import { PRIORIDADES_TAREFA, SITUACOES_TAREFA } from "../types/tarefa";
import { ehDataCivilValida } from "../utils/formatar-data";

export const LIMITE_DESCRICAO_TAREFA = 200;
export const LIMITE_OBSERVACOES_TAREFA = 4000;

export const tarefaSchema = z.object({
  descricao: z
    .string()
    .trim()
    .min(1, "A descrição é obrigatória.")
    .max(
      LIMITE_DESCRICAO_TAREFA,
      `A descrição deve ter no máximo ${LIMITE_DESCRICAO_TAREFA} caracteres.`,
    ),

  observacoes: z
    .string()
    .max(
      LIMITE_OBSERVACOES_TAREFA,
      `As observações devem ter no máximo ${LIMITE_OBSERVACOES_TAREFA} caracteres.`,
    ),

  situacao: z.enum(SITUACOES_TAREFA, {
    message: "Selecione uma situação válida.",
  }),
  prioridade: z.enum(PRIORIDADES_TAREFA, { message: "Selecione uma prioridade v\u00e1lida." }),
  dataVencimento: z.string().refine(
    (valor) => valor === "" || ehDataCivilValida(valor),
    "Informe uma data v\u00e1lida no formato dd/mm/aaaa.",
  ),
  etiquetaIds: z.array(z.number().int().positive()),
});

export type TarefaFormData = z.infer<typeof tarefaSchema>;
