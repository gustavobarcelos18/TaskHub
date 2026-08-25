import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { IconBotaoLink } from "@/components/ComponentesRoteador";
import { FormularioTarefa } from "@/features/tarefas/components/FormularioTarefa";

export default function NovaTarefaPage() {
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
      <Container maxWidth="md">
        <Stack spacing={4}>
          <Box>
            <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
              <Tooltip title="Voltar para tarefas">
                <IconBotaoLink
                  href="/tarefas"
                  aria-label="Voltar para tarefas"
                  sx={{ border: "1px solid", borderColor: "divider" }}
                >
                  <ArrowBackIcon />
                </IconBotaoLink>
              </Tooltip>

              <Typography variant="h2" component="h1">
                Nova tarefa
              </Typography>
            </Stack>

            <Typography
              variant="body2"
              color="text.secondary"
              sx={{ mt: 1, ml: { xs: 0, sm: 7 } }}
            >
              Preencha os dados para cadastrar uma nova tarefa.
            </Typography>
          </Box>

          <FormularioTarefa />
        </Stack>
      </Container>
    </Box>
  );
}