"use client";

import { useEffect, useMemo, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddIcon from "@mui/icons-material/Add";
import ClearIcon from "@mui/icons-material/Clear";
import { Alert, Box, Chip, FormControl, IconButton, InputAdornment, InputLabel, MenuItem, Paper, Select, Snackbar, Stack, TextField, Typography } from "@mui/material";
import { DataGrid, type GridColDef, type GridPaginationModel, type GridSortModel } from "@mui/x-data-grid";
import { BotaoLink } from "@/components/ComponentesRoteador";
import { PRAZOS_TAREFA, PRIORIDADES_TAREFA, SITUACOES_TAREFA, type ConsultaTarefas, type Etiqueta, type OrdenarTarefasPor, type Projeto, type SituacaoTarefa, type Tarefa, type TarefasPaginadas } from "../types/tarefa";
import { formatarDataCivil, formatarDataHora } from "../utils/formatar-data";
import { IndicadorSituacaoTarefa } from "./IndicadorSituacaoTarefa";
import { IndicadorPrioridadeTarefa } from "./IndicadorPrioridadeTarefa";
import { MenuAcoesTarefa } from "./MenuAcoesTarefa";

type Props = { resultado: TarefasPaginadas; consulta: ConsultaTarefas; etiquetas: Etiqueta[]; projetos: Projeto[] };
type Opcao = { valor: string; rotulo: string };

export function TabelaTarefas({ resultado, consulta, etiquetas, projetos }: Props) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const ordenarPor = consulta.ordenarPor ?? "ultimaAtualizacao";
  const direcao = consulta.direcao ?? "desc";
  const [feedbackAberto, setFeedbackAberto] = useState(searchParams.get("feedback") === "tarefaCriada");

  useEffect(() => {
    if (searchParams.get("feedback") !== "tarefaCriada") return;
    const parametros = new URLSearchParams(searchParams.toString());
    parametros.delete("feedback");
    const query = parametros.toString();
    router.replace(query ? `${pathname}?${query}` : pathname);
  }, [pathname, router, searchParams]);

  function atualizar(alteracoes: Record<string, string | undefined>) {
    const parametros = new URLSearchParams(searchParams.toString());
    Object.entries(alteracoes).forEach(([chave, valor]) => {
      if (valor) parametros.set(chave, valor);
      else parametros.delete(chave);
    });
    const query = parametros.toString();
    router.push(query ? `${pathname}?${query}` : pathname);
  }

  function ordenar(campo: OrdenarTarefasPor, novaDirecao: "asc" | "desc") {
    atualizar({ ordenarPor: campo, direcao: novaDirecao, pagina: "1" });
  }

  const semFiltros = !consulta.busca && !consulta.situacao && !consulta.prioridade && !consulta.prazo && !consulta.etiquetaId && !consulta.projetoId;
  if (resultado.totalItens === 0 && semFiltros) return <><EstadoVazio mensagem="Nenhuma tarefa cadastrada." criar /><FeedbackCriacao aberto={feedbackAberto} fechar={() => setFeedbackAberto(false)} /></>;

  return <><Stack spacing={2}>
    <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
      <CampoBusca key={consulta.busca ?? ""} buscaInicial={consulta.busca ?? ""} atualizar={atualizar} />
      <Filtro label="Situação" valor={consulta.situacao ?? ""} alterar={(situacao) => atualizar({ situacao, pagina: "1" })} opcoes={SITUACOES_TAREFA.map((v) => ({ valor: v, rotulo: v }))} />
      <Filtro label="Prioridade" valor={consulta.prioridade ?? ""} alterar={(prioridade) => atualizar({ prioridade, pagina: "1" })} opcoes={PRIORIDADES_TAREFA.map((v) => ({ valor: v, rotulo: v === "Media" ? "Média" : v }))} />
      <Filtro label="Prazo" valor={consulta.prazo ?? ""} alterar={(prazo) => atualizar({ prazo, pagina: "1" })} opcoes={PRAZOS_TAREFA.map((v) => ({ valor: v, rotulo: { vencidas: "Vencidas", vencemHoje: "Vencem hoje", proximas: "Próximas", semVencimento: "Sem vencimento" }[v] }))} />
      <Filtro label="Etiqueta" valor={consulta.etiquetaId ? String(consulta.etiquetaId) : ""} alterar={(etiquetaId) => atualizar({ etiquetaId, pagina: "1" })} opcoes={etiquetas.map((etiqueta) => ({ valor: String(etiqueta.id), rotulo: etiqueta.nome }))} />
      <Filtro label="Projeto" valor={consulta.projetoId ? String(consulta.projetoId) : ""} alterar={(projetoId) => atualizar({ projetoId, pagina: "1" })} opcoes={projetos.map((projeto) => ({ valor: String(projeto.id), rotulo: projeto.nome }))} />
    </Stack>
    {resultado.totalItens === 0 ? <EstadoVazio mensagem="Nenhuma tarefa encontrada com os filtros informados." /> : <Tabela resultado={resultado} ordenarPor={ordenarPor} direcao={direcao} ordenar={ordenar} atualizar={atualizar} />}
  </Stack><FeedbackCriacao aberto={feedbackAberto} fechar={() => setFeedbackAberto(false)} /></>;
}

function FeedbackCriacao({ aberto, fechar }: { aberto: boolean; fechar: () => void }) {
  return <Snackbar open={aberto} autoHideDuration={4000} onClose={fechar} anchorOrigin={{ vertical: "bottom", horizontal: "center" }}><Alert severity="success" variant="filled" onClose={fechar}>Tarefa criada com sucesso.</Alert></Snackbar>;
}

type TabelaProps = { resultado: TarefasPaginadas; ordenarPor: OrdenarTarefasPor; direcao: "asc" | "desc"; ordenar: (campo: OrdenarTarefasPor, direcao: "asc" | "desc") => void; atualizar: (alteracoes: Record<string, string | undefined>) => void };

function Tabela({ resultado, ordenarPor, direcao, ordenar, atualizar }: TabelaProps) {
  const colunas = useMemo<GridColDef<Tarefa>[]>(() => [
    { field: "descricao", headerName: "Descrição", flex: 1, minWidth: 250 },
    { field: "situacao", headerName: "Situação", minWidth: 150, renderCell: ({ row }) => <IndicadorSituacaoTarefa situacao={row.situacao} /> },
    { field: "prioridade", headerName: "Prioridade", minWidth: 120, renderCell: ({ row }) => <IndicadorPrioridadeTarefa prioridade={row.prioridade} /> },
    { field: "projeto", headerName: "Projeto", minWidth: 150, sortable: false, valueGetter: (_, row) => row.projeto?.nome ?? "—" },
    { field: "etiquetas", headerName: "Etiquetas", minWidth: 180, sortable: false, renderCell: ({ row }) => <Stack direction="row" spacing={0.5} sx={{ py: 1, overflow: "hidden" }}>{row.etiquetas.slice(0, 2).map((etiqueta) => <Chip key={etiqueta.id} label={etiqueta.nome} size="small" />)}{row.etiquetas.length > 2 && <Chip label={`+${row.etiquetas.length - 2}`} size="small" />}</Stack> },
    { field: "dataVencimento", headerName: "Vencimento", minWidth: 160, renderCell: ({ row }) => <Box sx={{ py: 1 }}><Typography variant="body2">{formatarDataCivil(row.dataVencimento)}</Typography><Typography variant="caption" color="text.secondary">{obterIndicadorPrazo(row.dataVencimento, row.situacao)}</Typography></Box> },
    { field: "ultimaAtualizacao", headerName: "Última atualização", minWidth: 185, valueGetter: (_, row) => row.modificadaEm ?? row.criadaEm, valueFormatter: (value) => formatarDataHora(value) },
    { field: "acoes", headerName: "Ações", sortable: false, filterable: false, align: "right", headerAlign: "right", width: 90, renderCell: ({ row }) => <MenuAcoesTarefa tarefa={row} /> },
  ], []);
  const sortModel: GridSortModel = [{ field: ordenarPor, sort: direcao }];
  const paginationModel: GridPaginationModel = { page: resultado.paginaAtual - 1, pageSize: resultado.tamanhoPagina };

  function alterarOrdenacao(modelo: GridSortModel) {
    const ordenacao = modelo[0];
    if (ordenacao?.sort) ordenar(ordenacao.field as OrdenarTarefasPor, ordenacao.sort);
  }

  function alterarPaginacao(modelo: GridPaginationModel) {
    atualizar({ pagina: String(modelo.page + 1), tamanhoPagina: String(modelo.pageSize) });
  }

  const alturaTabela = Math.min(620, Math.max(260, 156 + resultado.itens.length * 56));

  return <Paper variant="outlined" sx={{ height: alturaTabela, width: "100%" }}><DataGrid aria-label="Tabela de tarefas" columns={colunas} rows={resultado.itens} rowCount={resultado.totalItens} pagination paginationMode="server" paginationModel={paginationModel} pageSizeOptions={[10, 25, 50]} onPaginationModelChange={alterarPaginacao} sortingMode="server" sortModel={sortModel} onSortModelChange={alterarOrdenacao} disableRowSelectionOnClick sx={{ border: 0 }} /></Paper>;
}

function Filtro({ label, valor, alterar, opcoes }: { label: string; valor: string; alterar: (valor: string | undefined) => void; opcoes: Opcao[] }) {
  const id = `${label.toLowerCase()}-label`;
  return <FormControl sx={{ flex: "1 1 160px", minWidth: 160 }}><InputLabel id={id}>{label}</InputLabel><Select labelId={id} label={label} value={valor} onChange={(evento) => alterar(evento.target.value || undefined)}><MenuItem value="">Todos</MenuItem>{opcoes.map((opcao) => <MenuItem key={opcao.valor} value={opcao.valor}>{opcao.rotulo}</MenuItem>)}</Select></FormControl>;
}

function CampoBusca({ buscaInicial, atualizar }: { buscaInicial: string; atualizar: (alteracoes: Record<string, string | undefined>) => void }) {
  const [busca, setBusca] = useState(buscaInicial);
  return <TextField label="Pesquisar por descrição" value={busca} sx={{ flex: "2 1 280px", minWidth: 240 }} onChange={(e) => setBusca(e.target.value)} onKeyDown={(e) => { if (e.key === "Enter") atualizar({ busca: busca.trim() || undefined, pagina: "1" }); }} slotProps={{ input: { endAdornment: busca ? <InputAdornment position="end"><IconButton aria-label="Limpar pesquisa" edge="end" onClick={() => { setBusca(""); atualizar({ busca: undefined, pagina: "1" }); }}><ClearIcon /></IconButton></InputAdornment> : undefined } }} />;
}

function obterIndicadorPrazo(data: string | null, situacao: SituacaoTarefa): string {
  if (!data) return "Sem vencimento";
  if (situacao === "Concluída") return "Concluída";
  const hoje = obterDataAtualNegocio();
  return data < hoje ? "Vencida" : data === hoje ? "Vence hoje" : "Próxima";
}

function obterDataAtualNegocio(): string {
  const partes = new Intl.DateTimeFormat("en", { timeZone: "America/Sao_Paulo", year: "numeric", month: "2-digit", day: "2-digit" }).formatToParts();
  const valores = Object.fromEntries(partes.filter((parte) => parte.type !== "literal").map((parte) => [parte.type, parte.value]));
  return `${valores.year}-${valores.month}-${valores.day}`;
}

function EstadoVazio({ mensagem, criar = false }: { mensagem: string; criar?: boolean }) {
  return <Paper variant="outlined" sx={{ p: 6 }}><Stack spacing={2} sx={{ alignItems: "center" }}><Typography color="text.secondary">{mensagem}</Typography>{criar && <BotaoLink href="/tarefas/criar" variant="contained" startIcon={<AddIcon />}>Criar primeira tarefa</BotaoLink>}</Stack></Paper>;
}
