import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { IconBotaoLink } from "@/components/ComponentesRoteador";
import { DetalhesTarefaConteudo } from "@/features/tarefas/components/DetalhesTarefaConteudo";
import { buscarTarefa } from "@/features/tarefas/services/tarefa-service";

type Props = { params: Promise<{ id: string }> };

export default async function DetalhesTarefaPage({ params }: Props) {
  const { id } = await params;
  const tarefa = await buscarTarefa(Number(id));

  return <Box component="main" sx={{ minHeight: "100vh", bgcolor: "background.default", px: { xs: 2, sm: 4 }, py: { xs: 4, sm: 6 } }}><Container maxWidth="md"><Stack spacing={4}><Stack direction="row" spacing={2} sx={{ alignItems: "center" }}><IconBotaoLink href="/" aria-label="Voltar para o início" tooltip="Voltar para o início" sx={{ border: "1px solid", borderColor: "divider" }}><ArrowBackIcon /></IconBotaoLink><Box><Typography variant="h2" component="h1">Detalhes da tarefa</Typography><Typography variant="body2" color="text.secondary">Consulte as informações completas da tarefa.</Typography></Box></Stack><Paper variant="outlined" sx={{ p: { xs: 2.5, sm: 3 } }}><DetalhesTarefaConteudo tarefa={tarefa} /></Paper></Stack></Container></Box>;
}
