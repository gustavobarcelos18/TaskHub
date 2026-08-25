"use client";

import Alert from "@mui/material/Alert";
import Snackbar from "@mui/material/Snackbar";

type FeedbackSucessoProps = {
  aberto: boolean;
  mensagem: string | null;
  onFechar: () => void;
};

export function FeedbackSucesso({
  aberto,
  mensagem,
  onFechar,
}: FeedbackSucessoProps) {
  return (
    <Snackbar
      open={aberto}
      autoHideDuration={4000}
      onClose={onFechar}
      anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
    >
      <Alert
        severity="success"
        variant="filled"
        onClose={onFechar}
        sx={{ width: "100%" }}
      >
        {mensagem}
      </Alert>
    </Snackbar>
  );
}