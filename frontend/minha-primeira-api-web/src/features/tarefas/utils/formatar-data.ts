const formatadorDataHora = new Intl.DateTimeFormat("pt-BR", {
  dateStyle: "short",
  timeStyle: "short",
  timeZone: "America/Sao_Paulo",
});

function converterParaDataUtc(valor: string): Date {
  const possuiInformacaoDeFuso = /(?:Z|[+-]\d{2}:\d{2})$/.test(valor);

  const valorNormalizado = possuiInformacaoDeFuso ? valor : `${valor}Z`;

  return new Date(valorNormalizado);
}

export function formatarDataHora(valor: string | null): string {
  if (!valor) {
    return "—";
  }

  const data = converterParaDataUtc(valor);

  if (Number.isNaN(data.getTime())) {
    return "Data inválida";
  }

  return formatadorDataHora.format(data);
}
