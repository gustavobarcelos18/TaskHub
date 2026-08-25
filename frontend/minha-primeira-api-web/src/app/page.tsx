import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Container from "@mui/material/Container";
import Grid from "@mui/material/Grid";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { AreaAcaoLink } from "@/components/ComponentesRoteador";
import { obterResumoTarefas } from "@/features/tarefas/services/tarefa-service";
import type { ResumoTarefas, SituacaoTarefa } from "@/features/tarefas/types/tarefa";

type IndicadorDashboard = {
  titulo: string;
  valor: number;
  href: string;
};

function criarHrefTarefas(situacao?: SituacaoTarefa, prazo?: string): string {
  const parametros = new URLSearchParams();
  if (situacao) parametros.set("situacao", situacao);
  if (prazo) parametros.set("prazo", prazo);

  if (parametros.size === 0) return "/tarefas";

  return `/tarefas?${parametros.toString()}`;
}

function criarIndicadores(resumo: ResumoTarefas): IndicadorDashboard[] {
  return [
    { titulo: "Total", valor: resumo.total, href: criarHrefTarefas() },
    {
      titulo: "Pendentes",
      valor: resumo.pendentes,
      href: criarHrefTarefas("Pendente"),
    },
    {
      titulo: "Em andamento",
      valor: resumo.emAndamento,
      href: criarHrefTarefas("Em andamento"),
    },
    {
      titulo: "Concluídas",
      valor: resumo.concluidas,
      href: criarHrefTarefas("Concluída"),
    },
    { titulo: "Vencidas", valor: resumo.vencidas, href: criarHrefTarefas(undefined, "vencidas") },
    { titulo: "Vencem hoje", valor: resumo.vencemHoje, href: criarHrefTarefas(undefined, "vencemHoje") },
    { titulo: "Próximas", valor: resumo.proximas, href: criarHrefTarefas(undefined, "proximas") },
  ];
}

export default async function HomePage() {
  const resumo = await obterResumoTarefas();
  const indicadores = criarIndicadores(resumo);

  return (
    <Box
      component="main"
      sx={{
        minHeight: "100vh",
        display: "flex",
        bgcolor: "background.default",
        px: { xs: 2, sm: 4 },
        py: { xs: 4, sm: 6 },
      }}
    >
      <Container maxWidth="lg">
        <Stack spacing={4}>
          <Box>
            <Typography variant="h1" component="h1" gutterBottom>
              Gerenciador de Tarefas
            </Typography>

            <Typography variant="body1" color="text.secondary">
              Visão geral das suas tarefas ativas.
            </Typography>
          </Box>

          <Grid container spacing={3}>
            {indicadores.map((indicador) => (
              <Grid key={indicador.titulo} size={{ xs: 12, sm: 6, lg: 3 }}>
                <Card variant="outlined" sx={{ height: "100%" }}>
                  <AreaAcaoLink
                    href={indicador.href}
                    sx={{ height: "100%", alignItems: "stretch" }}
                  >
                    <CardContent>
                      <Typography
                        variant="overline"
                        color="text.secondary"
                        component="p"
                      >
                        {indicador.titulo}
                      </Typography>

                      <Typography variant="h2" component="p">
                        {indicador.valor}
                      </Typography>
                    </CardContent>
                  </AreaAcaoLink>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Stack>
      </Container>
    </Box>
  );
}
