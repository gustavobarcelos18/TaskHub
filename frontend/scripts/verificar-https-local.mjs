import { execFileSync } from "node:child_process";
import { existsSync } from "node:fs";
import { join } from "node:path";

try {
  const nextMkcert = join(
    process.env.LOCALAPPDATA ?? "",
    "mkcert",
    "mkcert-v1.4.4-windows-amd64.exe",
  );

  if (!existsSync(nextMkcert)) execFileSync("mkcert", ["-version"], { stdio: "ignore" });
} catch {
  throw new Error(
    "O HTTPS local requer mkcert instalado e confiado. Instale-o, execute 'mkcert -install' e reinicie o comando.",
  );
}
