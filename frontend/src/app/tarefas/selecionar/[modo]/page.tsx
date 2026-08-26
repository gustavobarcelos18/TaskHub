import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { IconBotaoLink } from "@/components/ComponentesRoteador";
import { SeletorTarefa, type ModoSelecaoTarefa } from "@/features/tarefas/components/SeletorTarefa";
import { listarTarefasServidor } from "@/features/tarefas/services/tarefas-server-service";
import type { ConsultaTarefas, TarefasPaginadas } from "@/features/tarefas/types/tarefa";

type SelecaoPageProps = { params: Promise<{ modo: string }>; searchParams: Promise<Record<string, string | string[] | undefined>> };
const configuracoes: Record<ModoSelecaoTarefa, { descricao: string; titulo: string }> = {
  detalhes: { titulo: "Consultar detalhes", descricao: "Selecione a tarefa que deseja visualizar." },
  editar: { titulo: "Editar tarefa", descricao: "Selecione a tarefa que deseja alterar." },
  historico: { titulo: "Consultar histórico", descricao: "Selecione a tarefa cujo histórico deseja consultar." },
};
function obterPagina(valor: string | string[] | undefined): number { const pagina = typeof valor === "string" ? Number(valor) : 1; return Number.isInteger(pagina) && pagina > 0 ? pagina : 1; }
function obterBusca(valor: string | string[] | undefined): string | undefined { return typeof valor === "string" && valor.trim() ? valor : undefined; }
function ehModoSelecaoTarefa(valor: string): valor is ModoSelecaoTarefa { return valor === "detalhes" || valor === "editar" || valor === "historico"; }

export default async function SelecaoTarefaPage({ params, searchParams }: SelecaoPageProps) {
  const [{ modo }, parametros] = await Promise.all([params, searchParams]);
  const modoValido = ehModoSelecaoTarefa(modo) ? modo : "detalhes";
  const consulta: ConsultaTarefas = { busca: obterBusca(parametros.busca), pagina: obterPagina(parametros.pagina), tamanhoPagina: 10 };
  let resultado: TarefasPaginadas | null = null;
  let erro: string | null = null;
  try { resultado = await listarTarefasServidor(consulta); } catch (causa) { erro = causa instanceof Error ? causa.message : "Não foi possível carregar as tarefas."; }
  const configuracao = configuracoes[modoValido];
  return <Box component="main" sx={{ minHeight: "100vh", bgcolor: "background.default", px: { xs: 2, sm: 4 }, py: { xs: 4, sm: 6 } }}><Container maxWidth="md"><Stack spacing={4}><Stack direction="row" spacing={2} sx={{ alignItems: "center" }}><IconBotaoLink href="/" aria-label="Voltar para o início" tooltip="Voltar para o início" sx={{ border: "1px solid", borderColor: "divider" }}><ArrowBackIcon /></IconBotaoLink><Box><Typography variant="h2" component="h1">{configuracao.titulo}</Typography><Typography variant="body2" color="text.secondary">{configuracao.descricao}</Typography></Box></Stack><SeletorTarefa modo={modoValido} consulta={consulta} resultado={resultado} erro={erro} /></Stack></Container></Box>;
}
