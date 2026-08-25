import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { Box, Container, Divider, Paper, Stack, Typography } from "@mui/material";
import { IconBotaoLink } from "@/components/ComponentesRoteador";
import { HistoricoTarefa } from "@/features/tarefas/components/HistoricoTarefa";
import { IndicadorSituacaoTarefa } from "@/features/tarefas/components/IndicadorSituacaoTarefa";
import { buscarTarefa, listarHistoricoTarefa } from "@/features/tarefas/services/tarefa-service";
import { formatarDataCivil, formatarDataHora } from "@/features/tarefas/utils/formatar-data";

type DetalhesTarefaPageProps = { params: Promise<{ id: string }> };
type CampoDetalheProps = { titulo: string; valor: React.ReactNode };

function CampoDetalhe({ titulo, valor }: CampoDetalheProps) { return <Stack spacing={0.5}><Typography variant="caption" color="text.secondary">{titulo}</Typography><Typography variant="body1" component="div">{valor}</Typography></Stack>; }

export default async function DetalhesTarefaPage({ params }: DetalhesTarefaPageProps) {
  const { id } = await params; const tarefaId = Number(id);
  const [tarefa, historico] = await Promise.all([buscarTarefa(tarefaId), listarHistoricoTarefa(tarefaId)]);
  return <Box component="main" sx={{ minHeight: "100vh", bgcolor: "background.default", px: { xs: 2, sm: 4 }, py: { xs: 4, sm: 6 } }}><Container maxWidth="md"><Stack spacing={4}><Stack direction="row" spacing={2} sx={{ alignItems: "center" }}><IconBotaoLink href="/tarefas" aria-label="Voltar para tarefas" tooltip="Voltar para tarefas" sx={{ border: "1px solid", borderColor: "divider" }}><ArrowBackIcon /></IconBotaoLink><Box><Typography variant="h2" component="h1">Detalhes da tarefa</Typography><Typography variant="body2" color="text.secondary">Acompanhe as informações e as alterações registradas.</Typography></Box></Stack><Paper variant="outlined" sx={{ p: { xs: 2.5, sm: 3 } }}><Stack spacing={3}><CampoDetalhe titulo="Descrição" valor={tarefa.descricao} /><Divider /><CampoDetalhe titulo="Situação" valor={<IndicadorSituacaoTarefa situacao={tarefa.situacao} />} /><Divider /><CampoDetalhe titulo="Prioridade" valor={tarefa.prioridade === "Media" ? "Média" : tarefa.prioridade} /><Divider /><CampoDetalhe titulo="Data de vencimento" valor={formatarDataCivil(tarefa.dataVencimento)} /><Divider /><CampoDetalhe titulo="Criada em" valor={formatarDataHora(tarefa.criadaEm)} /><CampoDetalhe titulo="Modificada em" valor={formatarDataHora(tarefa.modificadaEm)} /><CampoDetalhe titulo="Finalizada em" valor={formatarDataHora(tarefa.concluidaEm)} /></Stack></Paper><Stack spacing={2}><Typography variant="h3" component="h2">Histórico</Typography><HistoricoTarefa historico={historico} /></Stack></Stack></Container></Box>;
}
