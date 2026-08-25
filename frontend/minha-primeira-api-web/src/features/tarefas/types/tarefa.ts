export const SITUACOES_TAREFA = [
  "Pendente",
  "Em andamento",
  "Concluída",
] as const;

export type SituacaoTarefa = (typeof SITUACOES_TAREFA)[number];

export type Tarefa = {
  id: number;
  descricao: string;
  situacao: SituacaoTarefa;
  criadaEm: string;
  modificadaEm: string | null;
  situacaoAlteradaEm: string;
  concluidaEm: string | null;
  excluidaEm: string | null;
};
