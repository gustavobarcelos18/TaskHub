"use client";

import { useState, type ReactNode } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddIcon from "@mui/icons-material/Add";
import ClearIcon from "@mui/icons-material/Clear";
import { Box, FormControl, IconButton, InputAdornment, InputLabel, MenuItem, Paper, Select, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, TableSortLabel, TextField, Typography } from "@mui/material";
import { BotaoLink } from "@/components/ComponentesRoteador";
import { PRAZOS_TAREFA, PRIORIDADES_TAREFA, SITUACOES_TAREFA, type ConsultaTarefas, type OrdenarTarefasPor, type SituacaoTarefa, type TarefasPaginadas } from "../types/tarefa";
import { formatarDataCivil, formatarDataHora } from "../utils/formatar-data";
import { IndicadorSituacaoTarefa } from "./IndicadorSituacaoTarefa";
import { MenuAcoesTarefa } from "./MenuAcoesTarefa";

type Props = { resultado: TarefasPaginadas; consulta: ConsultaTarefas };
type Opcao = { valor: string; rotulo: string };

export function TabelaTarefas({ resultado, consulta }: Props) {
  const router = useRouter(); const pathname = usePathname(); const searchParams = useSearchParams();
  const ordenarPor = consulta.ordenarPor ?? "ultimaAtualizacao"; const direcao = consulta.direcao ?? "desc";
  function atualizar(alteracoes: Record<string, string | undefined>) { const parametros = new URLSearchParams(searchParams.toString()); Object.entries(alteracoes).forEach(([chave, valor]) => valor ? parametros.set(chave, valor) : parametros.delete(chave)); const query = parametros.toString(); router.push(query ? `${pathname}?${query}` : pathname); }
  function ordenar(campo: OrdenarTarefasPor) { atualizar({ ordenarPor: campo, direcao: ordenarPor === campo && direcao === "asc" ? "desc" : "asc", pagina: "1" }); }
  const semFiltros = !consulta.busca && !consulta.situacao && !consulta.prioridade && !consulta.prazo;
  if (resultado.totalItens === 0 && semFiltros) return <EstadoVazio mensagem="Nenhuma tarefa cadastrada." criar />;
  return <Stack spacing={2}>
    <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
      <CampoBusca key={consulta.busca ?? ""} buscaInicial={consulta.busca ?? ""} atualizar={atualizar} />
      <Filtro label="Situação" valor={consulta.situacao ?? ""} alterar={(situacao) => atualizar({ situacao, pagina: "1" })} opcoes={SITUACOES_TAREFA.map((v) => ({ valor: v, rotulo: v }))} />
      <Filtro label="Prioridade" valor={consulta.prioridade ?? ""} alterar={(prioridade) => atualizar({ prioridade, pagina: "1" })} opcoes={PRIORIDADES_TAREFA.map((v) => ({ valor: v, rotulo: v === "Media" ? "Média" : v }))} />
      <Filtro label="Prazo" valor={consulta.prazo ?? ""} alterar={(prazo) => atualizar({ prazo, pagina: "1" })} opcoes={PRAZOS_TAREFA.map((v) => ({ valor: v, rotulo: { vencidas: "Vencidas", vencemHoje: "Vencem hoje", proximas: "Próximas", semVencimento: "Sem vencimento" }[v] }))} />
    </Stack>
    {resultado.totalItens === 0 ? <EstadoVazio mensagem="Nenhuma tarefa encontrada com os filtros informados." /> : <Tabela resultado={resultado} ordenarPor={ordenarPor} direcao={direcao} ordenar={ordenar} atualizar={atualizar} />}
  </Stack>;
}

function Tabela({ resultado, ordenarPor, direcao, ordenar, atualizar }: { resultado: TarefasPaginadas; ordenarPor: OrdenarTarefasPor; direcao: "asc" | "desc"; ordenar: (campo: OrdenarTarefasPor) => void; atualizar: (alteracoes: Record<string, string | undefined>) => void }) {
  return <TableContainer component={Paper} variant="outlined"><Box sx={{ overflowX: "auto" }}><Table sx={{ minWidth: 1050 }} aria-label="Tabela de tarefas"><TableHead><TableRow><Cabecalho campo="descricao" ativo={ordenarPor} direcao={direcao} ordenar={ordenar}>Descrição</Cabecalho><Cabecalho campo="situacao" ativo={ordenarPor} direcao={direcao} ordenar={ordenar}>Situação</Cabecalho><Cabecalho campo="prioridade" ativo={ordenarPor} direcao={direcao} ordenar={ordenar}>Prioridade</Cabecalho><Cabecalho campo="dataVencimento" ativo={ordenarPor} direcao={direcao} ordenar={ordenar}>Vencimento</Cabecalho><Cabecalho campo="ultimaAtualizacao" ativo={ordenarPor} direcao={direcao} ordenar={ordenar}>Última atualização</Cabecalho><TableCell align="right">Ações</TableCell></TableRow></TableHead><TableBody>{resultado.itens.map((tarefa) => <TableRow key={tarefa.id} hover><TableCell component="th" scope="row">{tarefa.descricao}</TableCell><TableCell><IndicadorSituacaoTarefa situacao={tarefa.situacao} /></TableCell><TableCell>{tarefa.prioridade === "Media" ? "Média" : tarefa.prioridade}</TableCell><TableCell><Typography variant="body2">{formatarDataCivil(tarefa.dataVencimento)}</Typography><Typography variant="caption" color="text.secondary">{obterIndicadorPrazo(tarefa.dataVencimento, tarefa.situacao)}</Typography></TableCell><TableCell>{formatarDataHora(tarefa.modificadaEm ?? tarefa.criadaEm)}</TableCell><TableCell align="right"><MenuAcoesTarefa tarefa={tarefa} /></TableCell></TableRow>)}</TableBody></Table></Box><TablePagination component="div" count={resultado.totalItens} page={resultado.paginaAtual - 1} rowsPerPage={resultado.tamanhoPagina} rowsPerPageOptions={[10, 25, 50]} onPageChange={(_, pagina) => atualizar({ pagina: String(pagina + 1) })} onRowsPerPageChange={(e) => atualizar({ tamanhoPagina: e.target.value, pagina: "1" })} labelRowsPerPage="Itens por página:" /></TableContainer>;
}

function Filtro({ label, valor, alterar, opcoes }: { label: string; valor: string; alterar: (valor: string | undefined) => void; opcoes: Opcao[] }) { const id = `${label.toLowerCase()}-label`; return <FormControl sx={{ minWidth: 150 }}><InputLabel id={id}>{label}</InputLabel><Select labelId={id} label={label} value={valor} onChange={(evento) => alterar(evento.target.value || undefined)}><MenuItem value="">Todos</MenuItem>{opcoes.map((opcao) => <MenuItem key={opcao.valor} value={opcao.valor}>{opcao.rotulo}</MenuItem>)}</Select></FormControl>; }
function CampoBusca({ buscaInicial, atualizar }: { buscaInicial: string; atualizar: (alteracoes: Record<string, string | undefined>) => void }) { const [busca, setBusca] = useState(buscaInicial); return <TextField label="Pesquisar por descrição" value={busca} fullWidth onChange={(e) => setBusca(e.target.value)} onKeyDown={(e) => { if (e.key === "Enter") atualizar({ busca: busca.trim() || undefined, pagina: "1" }); }} slotProps={{ input: { endAdornment: busca ? <InputAdornment position="end"><IconButton aria-label="Limpar pesquisa" edge="end" onClick={() => { setBusca(""); atualizar({ busca: undefined, pagina: "1" }); }}><ClearIcon /></IconButton></InputAdornment> : undefined } }} />; }
function Cabecalho({ campo, ativo, direcao, ordenar, children }: { campo: OrdenarTarefasPor; ativo: OrdenarTarefasPor; direcao: "asc" | "desc"; ordenar: (campo: OrdenarTarefasPor) => void; children: ReactNode }) { return <TableCell sortDirection={ativo === campo ? direcao : false}><TableSortLabel active={ativo === campo} direction={ativo === campo ? direcao : "asc"} onClick={() => ordenar(campo)}>{children}</TableSortLabel></TableCell>; }
function obterIndicadorPrazo(data: string | null, situacao: SituacaoTarefa): string { if (!data) return "Sem vencimento"; if (situacao === "Concluída") return "Concluída"; const hoje = obterDataAtualNegocio(); return data < hoje ? "Vencida" : data === hoje ? "Vence hoje" : "Próxima"; }
function obterDataAtualNegocio(): string { const partes = new Intl.DateTimeFormat("en", { timeZone: "America/Sao_Paulo", year: "numeric", month: "2-digit", day: "2-digit" }).formatToParts(); const valores = Object.fromEntries(partes.filter((parte) => parte.type !== "literal").map((parte) => [parte.type, parte.value])); return `${valores.year}-${valores.month}-${valores.day}`; }
function EstadoVazio({ mensagem, criar = false }: { mensagem: string; criar?: boolean }) { return <Paper variant="outlined" sx={{ p: 6 }}><Stack spacing={2} sx={{ alignItems: "center" }}><Typography color="text.secondary">{mensagem}</Typography>{criar && <BotaoLink href="/tarefas/criar" variant="contained" startIcon={<AddIcon />}>Criar primeira tarefa</BotaoLink>}</Stack></Paper>; }
