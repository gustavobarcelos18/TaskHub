import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import AddIcon from "@mui/icons-material/Add";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import {
  BotaoLink,
  IconBotaoLink,
} from "@/components/ComponentesRoteador";
import { TabelaTarefas } from "@/features/tarefas/components/TabelaTarefas";
import { listarTarefas } from "@/features/tarefas/services/tarefa-service";

export default async function TarefasPage() {
  const tarefas = await listarTarefas();

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
          <Stack
            direction={{ xs: "column", sm: "row" }}
            spacing={2}
            sx={{
              alignItems: { xs: "flex-start", sm: "center" },
              justifyContent: "space-between",
            }}
          >
            <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
              <Tooltip title="Voltar para o início">
                <IconBotaoLink
                  href="/"
                  aria-label="Voltar para o início"
                  sx={{ border: "1px solid", borderColor: "divider" }}
                >
                  <ArrowBackIcon />
                </IconBotaoLink>
              </Tooltip>

              <Box>
                <Typography variant="h2" component="h1">
                  Tarefas
                </Typography>

                <Typography variant="body2" color="text.secondary">
                  Total de tarefas: {tarefas.length}
                </Typography>
              </Box>
            </Stack>

            <BotaoLink
              href="/tarefas/criar"
              variant="contained"
              startIcon={<AddIcon />}
            >
              Criar tarefa
            </BotaoLink>
          </Stack>

          <TabelaTarefas tarefas={tarefas} />
        </Stack>
      </Container>
    </Box>
  );
}