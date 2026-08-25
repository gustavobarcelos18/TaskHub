export type SituacaoTarefa = "Pendente" | "Em andamento" | "Concluída";

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
