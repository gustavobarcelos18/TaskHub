import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Paper from "@mui/material/Paper";
import Skeleton from "@mui/material/Skeleton";
import Stack from "@mui/material/Stack";

export default function TarefasLoading() {
  return (
    <Box component="main" sx={{ minHeight: "100vh", bgcolor: "background.default", px: { xs: 2, sm: 4 }, py: { xs: 4, sm: 6 } }}>
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
              <Skeleton variant="circular" width={40} height={40} />

              <Box>
                <Skeleton variant="text" width={160} height={40} />
                <Skeleton variant="text" width={120} height={20} />
              </Box>
            </Stack>

            <Skeleton variant="rounded" width={140} height={40} />
          </Stack>

          <Paper variant="outlined" sx={{ p: 2 }}>
            <Stack spacing={1}>
              <Skeleton variant="rounded" height={48} />
              <Skeleton variant="rounded" height={48} />
              <Skeleton variant="rounded" height={48} />
            </Stack>
          </Paper>
        </Stack>
      </Container>
    </Box>
  );
}