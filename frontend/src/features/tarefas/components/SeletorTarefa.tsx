"use client";

import { useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import HistoryIcon from "@mui/icons-material/History";
import VisibilityIcon from "@mui/icons-material/Visibility";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import Paper from "@mui/material/Paper";
import Snackbar from "@mui/material/Snackbar";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { BotaoLink } from "@/components/ComponentesRoteador";
import { listarHistoricoTarefa } from "../services/tarefa-service";
import type {
  ConsultaTarefas,
  HistoricoTarefa as HistoricoTarefaType,
  Tarefa,
  TarefasPaginadas,
} from "../types/tarefa";
import { formatarDataCivil } from "../utils/formatar-data";
import { DetalhesTarefaConteudo } from "./DetalhesTarefaConteudo";
import { DialogoEditarTarefa } from "./DialogoEditarTarefa";
import { HistoricoTarefa } from "./HistoricoTarefa";
import { IndicadorPrioridadeTarefa } from "./IndicadorPrioridadeTarefa";
import { IndicadorSituacaoTarefa } from "./IndicadorSituacaoTarefa";

export type ModoSelecaoTarefa = "detalhes" | "editar" | "historico";
type SeletorTarefaProps = {
  consulta: ConsultaTarefas;
  modo: ModoSelecaoTarefa;
  resultado: TarefasPaginadas;
};
const acoesPorModo: Record<
  ModoSelecaoTarefa,
  { icone: React.ReactNode; rotulo: string }
> = {
  detalhes: { rotulo: "Ver detalhes", icone: <VisibilityIcon /> },
  editar: { rotulo: "Editar", icone: <EditIcon /> },
  historico: { rotulo: "Ver histórico", icone: <HistoryIcon /> },
};

export function SeletorTarefa({
  consulta,
  modo,
  resultado,
}: SeletorTarefaProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [tarefaEmDetalhes, setTarefaEmDetalhes] = useState<Tarefa | null>(null);
  const [tarefaEmEdicao, setTarefaEmEdicao] = useState<Tarefa | null>(null);
  const [tarefaDoHistorico, setTarefaDoHistorico] = useState<Tarefa | null>(
    null,
  );
  const [historico, setHistorico] = useState<HistoricoTarefaType[] | null>(
    null,
  );
  const [erroHistorico, setErroHistorico] = useState<string | null>(null);
  const [carregandoHistorico, setCarregandoHistorico] = useState(false);
  const [mensagemSucesso, setMensagemSucesso] = useState<string | null>(null);

  function pesquisar(evento: React.FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const busca = String(
      new FormData(evento.currentTarget).get("busca") ?? "",
    ).trim();
    const parametros = new URLSearchParams(searchParams.toString());
    if (busca) parametros.set("busca", busca);
    else parametros.delete("busca");
    parametros.delete("pagina");
    router.push(`${pathname}?${parametros.toString()}`);
  }
  function mudarPagina(pagina: number) {
    const parametros = new URLSearchParams(searchParams.toString());
    parametros.set("pagina", String(pagina));
    router.push(`${pathname}?${parametros.toString()}`);
  }
  async function abrirHistorico(tarefa: Tarefa) {
    setTarefaDoHistorico(tarefa);
    setHistorico(null);
    setErroHistorico(null);
    setCarregandoHistorico(true);
    try {
      setHistorico(await listarHistoricoTarefa(tarefa.id));
    } catch (causa) {
      setErroHistorico(
        causa instanceof Error
          ? causa.message
          : "Não foi possível carregar o histórico.",
      );
    } finally {
      setCarregandoHistorico(false);
    }
  }
  function fecharHistorico() {
    setTarefaDoHistorico(null);
    setHistorico(null);
    setErroHistorico(null);
  }
  function acionarTarefa(tarefa: Tarefa) {
    if (modo === "detalhes") setTarefaEmDetalhes(tarefa);
    else if (modo === "editar") setTarefaEmEdicao(tarefa);
    else void abrirHistorico(tarefa);
  }

  const semBusca = !consulta.busca;
  return (
    <Stack spacing={3}>
      <Box component="form" onSubmit={pesquisar}>
        <TextField
          name="busca"
          label="Buscar por descrição"
          defaultValue={consulta.busca ?? ""}
          fullWidth
          slotProps={{
            input: { endAdornment: <Button type="submit">Buscar</Button> },
          }}
        />
      </Box>
      {resultado.totalItens === 0 ? (
        <Paper variant="outlined" sx={{ p: 4, textAlign: "center" }}>
          <Stack spacing={2} sx={{ alignItems: "center" }}>
            <Typography color="text.secondary">
              {semBusca
                ? "Ainda não existem tarefas cadastradas."
                : "Nenhuma tarefa corresponde à busca informada."}
            </Typography>
            {semBusca && (
              <BotaoLink href="/tarefas/criar" variant="contained">
                Criar tarefa
              </BotaoLink>
            )}
          </Stack>
        </Paper>
      ) : (
        <Stack spacing={2}>
          {resultado.itens.map((tarefa) => (
            <Paper
              key={tarefa.id}
              variant="outlined"
              sx={{ p: { xs: 2, sm: 2.5 } }}
            >
              <Stack spacing={2}>
                <Stack
                  direction={{ xs: "column", sm: "row" }}
                  spacing={2}
                  sx={{
                    alignItems: { sm: "center" },
                    justifyContent: "space-between",
                  }}
                >
                  <Box>
                    <Typography variant="h5" component="h2">
                      {tarefa.descricao}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {tarefa.projeto?.nome ?? "Sem projeto"}
                      {tarefa.dataVencimento
                        ? ` · Vencimento: ${formatarDataCivil(tarefa.dataVencimento)}`
                        : ""}
                    </Typography>
                  </Box>
                  <Button
                    variant="outlined"
                    startIcon={acoesPorModo[modo].icone}
                    onClick={() => acionarTarefa(tarefa)}
                  >
                    {acoesPorModo[modo].rotulo}
                  </Button>
                </Stack>
                <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
                  <IndicadorSituacaoTarefa situacao={tarefa.situacao} />
                  <IndicadorPrioridadeTarefa prioridade={tarefa.prioridade} />
                </Stack>
              </Stack>
            </Paper>
          ))}
        </Stack>
      )}
      {resultado.totalPaginas > 1 && (
        <Stack
          direction="row"
          spacing={1}
          sx={{ justifyContent: "space-between", alignItems: "center" }}
        >
          <Button
            disabled={resultado.paginaAtual === 1}
            onClick={() => mudarPagina(resultado.paginaAtual - 1)}
          >
            Anterior
          </Button>
          <Typography variant="body2" color="text.secondary">
            Página {resultado.paginaAtual} de {resultado.totalPaginas}
          </Typography>
          <Button
            disabled={resultado.paginaAtual === resultado.totalPaginas}
            onClick={() => mudarPagina(resultado.paginaAtual + 1)}
          >
            Próxima
          </Button>
        </Stack>
      )}
      {tarefaEmDetalhes && (
        <Dialog
          open
          onClose={() => setTarefaEmDetalhes(null)}
          maxWidth="md"
          fullWidth
          aria-labelledby="dialogo-detalhes-titulo"
        >
          <DialogTitle id="dialogo-detalhes-titulo" component="h2">
            Detalhes da tarefa
            <IconButton
              aria-label="Fechar detalhes"
              onClick={() => setTarefaEmDetalhes(null)}
              sx={{ position: "absolute", right: 8, top: 8 }}
            >
              <CloseIcon />
            </IconButton>
          </DialogTitle>
          <DialogContent dividers>
            <DetalhesTarefaConteudo tarefa={tarefaEmDetalhes} />
          </DialogContent>
        </Dialog>
      )}
      {tarefaDoHistorico && (
        <Dialog
          open
          onClose={fecharHistorico}
          maxWidth="md"
          fullWidth
          aria-labelledby="dialogo-historico-titulo"
        >
          <DialogTitle id="dialogo-historico-titulo" component="h2">
            Histórico: {tarefaDoHistorico.descricao}
            <IconButton
              aria-label="Fechar histórico"
              onClick={fecharHistorico}
              sx={{ position: "absolute", right: 8, top: 8 }}
            >
              <CloseIcon />
            </IconButton>
          </DialogTitle>
          <DialogContent dividers>
            {carregandoHistorico && (
              <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                <CircularProgress size={20} />
                <Typography color="text.secondary">
                  Carregando histórico...
                </Typography>
              </Stack>
            )}
            {erroHistorico && <Alert severity="error">{erroHistorico}</Alert>}
            {historico && <HistoricoTarefa historico={historico} />}
          </DialogContent>
        </Dialog>
      )}
      {tarefaEmEdicao && (
        <DialogoEditarTarefa
          tarefa={tarefaEmEdicao}
          open
          onOpenChange={(aberto) => {
            if (!aberto) setTarefaEmEdicao(null);
          }}
          onSuccess={setMensagemSucesso}
        />
      )}
      <Snackbar
        open={Boolean(mensagemSucesso)}
        autoHideDuration={4000}
        onClose={() => setMensagemSucesso(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          severity="success"
          variant="filled"
          onClose={() => setMensagemSucesso(null)}
        >
          {mensagemSucesso}
        </Alert>
      </Snackbar>
    </Stack>
  );
}
