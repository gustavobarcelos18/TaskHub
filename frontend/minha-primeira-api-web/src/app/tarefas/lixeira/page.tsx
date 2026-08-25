import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { IconBotaoLink } from "@/components/ComponentesRoteador";
import { TabelaLixeira } from "@/features/tarefas/components/TabelaLixeira";
import { listarTarefasExcluidas } from "@/features/tarefas/services/tarefa-service";

export default async function LixeiraPage() {
  const tarefas = await listarTarefasExcluidas();

  return (
    <Box
      component="main"
      sx={{
        minHeight: "100vh",
        bgcolor: "background.default",
        px: { xs: 2, sm: 4 },
        py: { xs: 4, sm: 6 },
      }}
    >
      <Container maxWidth="lg">
        <Stack spacing={4}>
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <IconBotaoLink
              href="/tarefas"
              aria-label="Voltar para tarefas"
              tooltip="Voltar para tarefas"
              sx={{ border: "1px solid", borderColor: "divider" }}
            >
              <ArrowBackIcon />
            </IconBotaoLink>

            <Box>
              <Typography variant="h2" component="h1">
                Lixeira
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Tarefas excluídas podem ser restauradas ou removidas permanentemente.
              </Typography>
            </Box>
          </Stack>

          <TabelaLixeira tarefas={tarefas} />
        </Stack>
      </Container>
    </Box>
  );
}
