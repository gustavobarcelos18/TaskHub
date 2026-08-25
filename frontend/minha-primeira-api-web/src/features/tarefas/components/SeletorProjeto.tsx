"use client";

import { useEffect, useState } from "react";
import { Controller, type Control } from "react-hook-form";
import { Autocomplete, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Stack, TextField } from "@mui/material";
import type { TarefaFormData } from "../schemas/tarefa-schema";
import { criarProjeto, excluirProjeto, listarProjetos } from "../services/projeto-service";
import type { Projeto } from "../types/tarefa";

type Props = { control: Control<TarefaFormData> };

export function SeletorProjeto({ control }: Props) {
  const [opcoes, setOpcoes] = useState<Projeto[]>([]); const [novo, setNovo] = useState(""); const [erro, setErro] = useState<string | null>(null); const [gerenciar, setGerenciar] = useState(false);
  useEffect(() => { listarProjetos().then(setOpcoes).catch((causa: unknown) => setErro(causa instanceof Error ? causa.message : "Não foi possível carregar os projetos.")); }, []);
  async function criar(alterar: (id: number | null) => void) { if (!novo.trim()) return; try { const projeto = await criarProjeto(novo); setOpcoes((itens) => [...itens, projeto].sort((a, b) => a.nome.localeCompare(b.nome))); alterar(projeto.id); setNovo(""); setErro(null); } catch (causa) { setErro(causa instanceof Error ? causa.message : "Não foi possível criar o projeto."); } }
  async function remover(projeto: Projeto) { if (!window.confirm(`Excluir "${projeto.nome}"? As tarefas permanecerão e ficarão sem projeto.`)) return; try { await excluirProjeto(projeto.id); setOpcoes((itens) => itens.filter((item) => item.id !== projeto.id)); setErro(null); } catch (causa) { setErro(causa instanceof Error ? causa.message : "Não foi possível excluir o projeto."); } }
  return <><Controller name="projetoId" control={control} render={({ field }) => <Stack spacing={1}><Autocomplete options={opcoes} value={opcoes.find((opcao) => opcao.id === field.value) ?? null} onChange={(_, valor) => field.onChange(valor?.id ?? null)} getOptionLabel={(opcao) => opcao.nome} isOptionEqualToValue={(a, b) => a.id === b.id} renderInput={(params) => <TextField {...params} label="Projeto" helperText="Opcional. Selecione um projeto ou deixe sem projeto." />} /><Stack direction={{ xs: "column", sm: "row" }} spacing={1}><TextField size="small" label="Novo projeto" value={novo} onChange={(evento) => setNovo(evento.target.value)} slotProps={{ htmlInput: { maxLength: 100 } }} /><Button variant="outlined" onClick={() => criar(field.onChange)}>Criar projeto</Button><Button variant="text" onClick={() => setGerenciar(true)}>Gerenciar projetos</Button></Stack>{erro && <TextField error value={erro} slotProps={{ input: { readOnly: true } }} />}</Stack>} /><Dialog open={gerenciar} onClose={() => setGerenciar(false)} fullWidth maxWidth="xs"><DialogTitle>Gerenciar projetos</DialogTitle><DialogContent><DialogContentText sx={{ mb: 2 }}>Excluir um projeto não exclui as tarefas. Elas ficarão sem projeto.</DialogContentText><Stack spacing={1}>{opcoes.map((projeto) => <Stack key={projeto.id} direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}><span>{projeto.nome}</span><Button color="error" onClick={() => remover(projeto)}>Excluir</Button></Stack>)}</Stack></DialogContent><DialogActions><Button onClick={() => setGerenciar(false)}>Fechar</Button></DialogActions></Dialog></>;
}
