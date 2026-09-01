type ApiProblemDetails = {
  detail?: string;
  errors?: Record<string, string[]>;
  title?: string;
};

export async function criarErroHttp(
  resposta: Response,
  operacao: string,
): Promise<Error> {
  if (resposta.status >= 500) {
    return new Error(
      "O serviço está indisponível. Tente novamente em instantes.",
    );
  }

  const conteudo: unknown = await resposta.json().catch(() => null);
  const problema = ehApiProblemDetails(conteudo) ? conteudo : null;
  const mensagemValidacao = problema?.errors
    ? Object.values(problema.errors).flat().find(Boolean)
    : undefined;
  const mensagem = mensagemValidacao ?? problema?.detail ?? problema?.title;

  return new Error(
    mensagem ?? `Não foi possível ${operacao}. Status: ${resposta.status}.`,
  );
}

function ehApiProblemDetails(valor: unknown): valor is ApiProblemDetails {
  if (!valor || typeof valor !== "object") return false;

  const problema = valor as Record<string, unknown>;

  return (
    (problema.detail === undefined || typeof problema.detail === "string") &&
    (problema.title === undefined || typeof problema.title === "string") &&
    (problema.errors === undefined || errosSaoValidos(problema.errors))
  );
}

function errosSaoValidos(valor: unknown): valor is Record<string, string[]> {
  if (!valor || typeof valor !== "object") return false;

  return Object.values(valor).every(
    (mensagens) =>
      Array.isArray(mensagens) &&
      mensagens.every((mensagem) => typeof mensagem === "string"),
  );
}
