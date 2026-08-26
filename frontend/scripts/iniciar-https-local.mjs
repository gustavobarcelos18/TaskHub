import { execFileSync, spawn } from "node:child_process";

execFileSync(process.execPath, ["scripts/verificar-https-local.mjs"], {
  stdio: "inherit",
});

const nodeOptions = `${process.env.NODE_OPTIONS ?? ""} --use-system-ca`.trim();
const processo = spawn(
  process.execPath,
  ["node_modules/next/dist/bin/next", "dev", "--experimental-https"],
  {
    env: { ...process.env, NODE_OPTIONS: nodeOptions },
    stdio: "inherit",
  },
);

processo.on("exit", (codigo) => process.exit(codigo ?? 1));
