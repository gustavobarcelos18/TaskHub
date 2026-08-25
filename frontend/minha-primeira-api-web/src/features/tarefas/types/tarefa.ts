export const SITUACOES_TAREFA = [
  "Pendente",
  "Em andamento",
  "Concluída",
] as const;

export type SituacaoTarefa = (typeof SITUACOES_TAREFA)[number];

export const PRIORIDADES_TAREFA = ["Baixa", "Media", "Alta"] as const;
export type PrioridadeTarefa = (typeof PRIORIDADES_TAREFA)[number];

export const PRAZOS_TAREFA = ["vencidas", "vencemHoje", "proximas", "semVencimento"] as const;
export type PrazoTarefa = (typeof PRAZOS_TAREFA)[number];

export type Tarefa = {
  id: number;
  descricao: string;
  observacoes: string | null;
  situacao: SituacaoTarefa;
  prioridade: PrioridadeTarefa;
  dataVencimento: string | null;
  criadaEm: string;
  modificadaEm: string | null;
  situacaoAlteradaEm: string;
  concluidaEm: string | null;
  excluidaEm: string | null;
  projeto: Projeto | null;
  etiquetas: Etiqueta[];
};

export type Etiqueta = { id: number; nome: string };
export type Projeto = { id: number; nome: string };

export const TIPOS_HISTORICO_TAREFA = [
  "Criacao",
  "AlteracaoDescricao",
  "AlteracaoObservacoes",
  "AlteracaoEtiquetas",
  "AlteracaoProjeto",
  "AlteracaoPrioridade",
  "AlteracaoDataVencimento",
  "AlteracaoSituacao",
  "Conclusao",
  "Reabertura",
  "Exclusao",
  "Restauracao",
] as const;

export type TipoHistoricoTarefa = (typeof TIPOS_HISTORICO_TAREFA)[number];

export type HistoricoTarefa = {
  id: number;
  tipo: TipoHistoricoTarefa;
  campo: string | null;
  valorAnterior: string | null;
  valorNovo: string | null;
  criadoEm: string;
};

export type OrdenarTarefasPor = "descricao" | "situacao" | "prioridade" | "dataVencimento" | "ultimaAtualizacao";
export type DirecaoOrdenacao = "asc" | "desc";

export type ConsultaTarefas = {
  busca?: string;
  situacao?: SituacaoTarefa;
  prioridade?: PrioridadeTarefa;
  prazo?: PrazoTarefa;
  etiquetaId?: number;
  projetoId?: number;
  ordenarPor?: OrdenarTarefasPor;
  direcao?: DirecaoOrdenacao;
  pagina?: number;
  tamanhoPagina?: number;
};

export type TarefasPaginadas = {
  itens: Tarefa[];
  paginaAtual: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type ResumoTarefas = {
  total: number;
  pendentes: number;
  emAndamento: number;
  concluidas: number;
  vencidas: number;
  vencemHoje: number;
  proximas: number;
};
