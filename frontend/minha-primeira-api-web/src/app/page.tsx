import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";
import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import { BotaoLink } from "@/components/ComponentesRoteador";

export default function HomePage() {
  return (
    <Box
      component="main"
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        bgcolor: "background.default",
        px: 4,
      }}
    >
      <Container maxWidth="md">
        <Paper
          variant="outlined"
          sx={{ p: { xs: 4, sm: 6 }, textAlign: "center" }}
        >
          <Typography variant="h1" component="h1" gutterBottom>
            Gerenciador de Tarefas
          </Typography>

          <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
            Organize suas atividades de forma simples e eficiente.
          </Typography>

          <BotaoLink
            href="/tarefas"
            variant="contained"
            size="large"
            endIcon={<ArrowForwardIcon />}
          >
            Acessar tarefas
          </BotaoLink>
        </Paper>
      </Container>
    </Box>
  );
}