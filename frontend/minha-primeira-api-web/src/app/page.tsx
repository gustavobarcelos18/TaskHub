import AddTaskIcon from "@mui/icons-material/AddTask";
import EditNoteIcon from "@mui/icons-material/EditNote";
import FormatListBulletedIcon from "@mui/icons-material/FormatListBulleted";
import HistoryIcon from "@mui/icons-material/History";
import VisibilityIcon from "@mui/icons-material/Visibility";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Container from "@mui/material/Container";
import Grid from "@mui/material/Grid";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { AreaAcaoLink } from "@/components/ComponentesRoteador";

type AcaoInicial = { descricao: string; href: string; icone: React.ReactNode; titulo: string };

const acoesIniciais: AcaoInicial[] = [
  { titulo: "Criar Tarefa", descricao: "Cadastre uma nova tarefa no sistema.", href: "/tarefas/criar", icone: <AddTaskIcon fontSize="large" /> },
  { titulo: "Detalhes", descricao: "Consulte todas as informações de uma tarefa.", href: "/tarefas/selecionar/detalhes", icone: <VisibilityIcon fontSize="large" /> },
  { titulo: "Editar", descricao: "Selecione uma tarefa e altere suas informações.", href: "/tarefas/selecionar/editar", icone: <EditNoteIcon fontSize="large" /> },
  { titulo: "Histórico", descricao: "Consulte as alterações realizadas em uma tarefa.", href: "/tarefas/selecionar/historico", icone: <HistoryIcon fontSize="large" /> },
  { titulo: "Grade de tarefas", descricao: "Visualize, filtre e organize todas as tarefas cadastradas.", href: "/tarefas", icone: <FormatListBulletedIcon fontSize="large" /> },
];

export default function HomePage() {
  return <Box component="main" sx={{ minHeight: "100vh", bgcolor: "background.default", px: { xs: 2, sm: 3 }, py: { xs: 3, sm: 4 } }}><Container maxWidth="xl"><Stack spacing={4}><Box><Typography variant="h1" component="h1" gutterBottom>TaskHub</Typography><Typography variant="body1" color="text.secondary">O que você deseja fazer?</Typography></Box><Grid container spacing={3}>{acoesIniciais.map((acao) => <Grid key={acao.titulo} size={{ xs: 12, sm: 6 }}><Card variant="outlined" sx={{ height: "100%", transition: "box-shadow 150ms ease, transform 150ms ease", "&:hover": { boxShadow: 4, transform: "translateY(-2px)" } }}><AreaAcaoLink href={acao.href} sx={{ height: "100%", alignItems: "stretch" }}><CardContent sx={{ p: { xs: 3, sm: 4 } }}><Stack spacing={2}><Box color="primary.main" aria-hidden="true">{acao.icone}</Box><Typography variant="h3" component="h2">{acao.titulo}</Typography><Typography color="text.secondary">{acao.descricao}</Typography></Stack></CardContent></AreaAcaoLink></Card></Grid>)}</Grid></Stack></Container></Box>;
}
