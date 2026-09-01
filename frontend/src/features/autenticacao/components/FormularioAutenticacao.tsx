"use client";

import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Alert,
  Box,
  Button,
  Container,
  IconButton,
  InputAdornment,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useSessao } from "./SessaoProvider";
import { cadastrar, login } from "../services/sessao-service";

const schemaLogin = z.object({
  email: z.string().email("Informe um e-mail válido."),
  senha: z.string().min(1, "Informe a senha."),
});

const schemaCadastro = schemaLogin.extend({
  senha: z
    .string()
    .min(12, "A senha deve ter no mínimo 12 caracteres.")
    .regex(/[A-Z]/, "A senha deve conter uma letra maiúscula.")
    .regex(/[a-z]/, "A senha deve conter uma letra minúscula.")
    .regex(/\d/, "A senha deve conter um número.")
    .regex(/[^A-Za-z0-9]/, "A senha deve conter um símbolo."),
});

type Dados = z.infer<typeof schemaLogin>;

export function FormularioAutenticacao({
  modo,
}: {
  modo: "login" | "cadastro";
}) {
  const router = useRouter();
  const { atualizarSessao } = useSessao();
  const [erro, setErro] = useState<string | null>(null);
  const [senhaVisivel, setSenhaVisivel] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<Dados>({
    resolver: zodResolver(modo === "cadastro" ? schemaCadastro : schemaLogin),
  });
  const titulo = modo === "login" ? "Entrar no TaskHub" : "Criar conta";

  const enviar = async (dados: Dados) => {
    try {
      setErro(null);
      if (modo === "login") {
        await login(dados.email, dados.senha);
      } else {
        await cadastrar(dados.email, dados.senha);
      }
      await atualizarSessao();
      router.replace("/");
      router.refresh();
    } catch (causa) {
      setErro(
        causa instanceof Error
          ? causa.message
          : "Não foi possível concluir a operação.",
      );
    }
  };

  return (
    <Box
      component="main"
      sx={{ minHeight: "100vh", display: "grid", placeItems: "center", p: 2 }}
    >
      <Container maxWidth="xs">
        <Paper variant="outlined" sx={{ p: 4 }}>
          <Stack component="form" spacing={2} onSubmit={handleSubmit(enviar)}>
            <Typography variant="h2" component="h1">
              {titulo}
            </Typography>
            <Typography color="text.secondary">
              {modo === "login"
                ? "Use sua conta para acessar suas tarefas."
                : "Cadastre-se para organizar suas próprias tarefas."}
            </Typography>
            {erro && <Alert severity="error">{erro}</Alert>}
            <TextField
              type="email"
              label="E-mail"
              autoComplete="email"
              error={Boolean(errors.email)}
              helperText={errors.email?.message}
              {...register("email")}
            />
            <TextField
              label="Senha"
              type={senhaVisivel ? "text" : "password"}
              autoComplete={
                modo === "login" ? "current-password" : "new-password"
              }
              error={Boolean(errors.senha)}
              helperText={
                errors.senha?.message ??
                (modo === "cadastro"
                  ? "Mínimo 12 caracteres, maiúscula, minúscula, número e símbolo."
                  : undefined)
              }
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        aria-label={
                          senhaVisivel ? "Ocultar senha" : "Mostrar senha"
                        }
                        edge="end"
                        onClick={() => setSenhaVisivel((visivel) => !visivel)}
                      >
                        {senhaVisivel ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }}
              {...register("senha")}
            />
            <Button type="submit" variant="contained" disabled={isSubmitting}>
              {modo === "login" ? "Entrar" : "Cadastrar"}
            </Button>
            <Button
              component={Link}
              href={modo === "login" ? "/cadastro" : "/login"}
              variant="text"
            >
              {modo === "login" ? "Criar uma conta" : "Já tenho uma conta"}
            </Button>
          </Stack>
        </Paper>
      </Container>
    </Box>
  );
}
