import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import AddIcon from "@mui/icons-material/Add";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutlined";
import {
  BotaoLink,
  IconBotaoLink,
} from "@/components/ComponentesRoteador";
import { TabelaTarefas } from "@/features/tarefas/components/TabelaTarefas";
import { listarTarefas } from "@/features/tarefas/services/tarefa-service";
import { PRAZOS_TAREFA, PRIORIDADES_TAREFA, SITUACOES_TAREFA, type ConsultaTarefas, type DirecaoOrdenacao, type OrdenarTarefasPor, type PrazoTarefa, type PrioridadeTarefa, type SituacaoTarefa } from "@/features/tarefas/types/tarefa";

type TarefasPageProps = {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

function obterNumero(valor: string | string[] | undefined): number | undefined {
  const numero = Number(valor);
  return Number.isInteger(numero) ? numero : undefined;
}

function obterValorValido<T extends string>(valor: string | string[] | undefined, valores: readonly T[]): T | undefined {
  return typeof valor === "string" && valores.includes(valor as T) ? valor as T : undefined;
}

export default async function TarefasPage({ searchParams }: TarefasPageProps) {
  const parametros = await searchParams;
  const consulta: ConsultaTarefas = {
    busca: typeof parametros.busca === "string" ? parametros.busca : undefined,
    situacao: obterValorValido<SituacaoTarefa>(parametros.situacao, SITUACOES_TAREFA),
    prioridade: obterValorValido<PrioridadeTarefa>(parametros.prioridade, PRIORIDADES_TAREFA),
    prazo: obterValorValido<PrazoTarefa>(parametros.prazo, PRAZOS_TAREFA),
    ordenarPor: obterValorValido<OrdenarTarefasPor>(parametros.ordenarPor, ["descricao", "situacao", "prioridade", "dataVencimento", "ultimaAtualizacao"]),
    direcao: obterValorValido<DirecaoOrdenacao>(parametros.direcao, ["asc", "desc"]),
    pagina: obterNumero(parametros.pagina),
    tamanhoPagina: obterNumero(parametros.tamanhoPagina),
  };
  const tarefas = await listarTarefas(consulta);

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
              <IconBotaoLink
                href="/"
                aria-label="Voltar para o início"
                tooltip="Voltar para o início"
                sx={{ border: "1px solid", borderColor: "divider" }}
              >
                <ArrowBackIcon />
              </IconBotaoLink>

              <Box>
                <Typography variant="h2" component="h1">
                  Tarefas
                </Typography>

                <Typography variant="body2" color="text.secondary">
                  Total de tarefas: {tarefas.totalItens}
                </Typography>
              </Box>
            </Stack>

            <Stack direction="row" spacing={1}>
              <BotaoLink
                href="/tarefas/lixeira"
                variant="outlined"
                color="error"
                startIcon={<DeleteOutlineIcon />}
              >
                Lixeira
              </BotaoLink>

              <BotaoLink
                href="/tarefas/criar"
                variant="contained"
                startIcon={<AddIcon />}
              >
                Criar tarefa
              </BotaoLink>
            </Stack>
          </Stack>

          <TabelaTarefas resultado={tarefas} consulta={consulta} />
        </Stack>
      </Container>
    </Box>
  );
}
