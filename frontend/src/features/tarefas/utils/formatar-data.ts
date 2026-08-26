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

export function formatarDataCivil(valor: string | null): string {
  if (!valor) return "Sem vencimento";

  const partes = /^(\d{4})-(\d{2})-(\d{2})$/.exec(valor);
  if (!partes) return "Data inv\u00e1lida";

  return `${partes[3]}/${partes[2]}/${partes[1]}`;
}

export function ehDataCivilValida(valor: string): boolean {
  const partes = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(valor);

  if (!partes) return false;

  const dia = Number(partes[1]);
  const mes = Number(partes[2]);
  const ano = Number(partes[3]);
  const data = new Date(Date.UTC(ano, mes - 1, dia));

  return (
    data.getUTCFullYear() === ano &&
    data.getUTCMonth() === mes - 1 &&
    data.getUTCDate() === dia
  );
}

export function converterDataCivilParaApi(valor: string): string | null {
  if (!valor) return null;

  const [dia, mes, ano] = valor.split("/");
  return `${ano}-${mes}-${dia}`;
}

export function converterDataParaFormulario(valor: string | null): string {
  if (!valor) return "";

  const [ano, mes, dia] = valor.split("-");
  return `${dia}/${mes}/${ano}`;
}

export function mascararDataCivil(valor: string): string {
  const digitos = valor.replace(/\D/g, "").slice(0, 8);

  if (digitos.length <= 2) return digitos;
  if (digitos.length <= 4) return `${digitos.slice(0, 2)}/${digitos.slice(2)}`;

  return `${digitos.slice(0, 2)}/${digitos.slice(2, 4)}/${digitos.slice(4)}`;
}
