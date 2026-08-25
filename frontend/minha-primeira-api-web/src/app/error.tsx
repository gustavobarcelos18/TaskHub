"use client";

import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Container from "@mui/material/Container";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

type DashboardErrorProps = {
  reset: () => void;
};

export default function DashboardError({ reset }: DashboardErrorProps) {
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
        <Paper variant="outlined" sx={{ p: 4 }}>
          <Stack spacing={3}>
            <Alert severity="error" variant="filled">
              <Stack spacing={1}>
                <Typography variant="h6" component="h1">
                  Não foi possível carregar o resumo das tarefas
                </Typography>

                <Typography variant="body2">
                  Verifique se a API está em execução e tente novamente.
                </Typography>
              </Stack>
            </Alert>

            <Box>
              <Button onClick={reset} variant="contained" color="error">
                Tentar novamente
              </Button>
            </Box>
          </Stack>
        </Paper>
      </Container>
    </Box>
  );
}
