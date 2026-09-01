"use client";

import { AppRouterCacheProvider } from "@mui/material-nextjs/v16-appRouter";
import { ThemeProvider as MuiThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import { theme } from "./theme";
import { SessaoProvider } from "@/features/autenticacao/components/SessaoProvider";
import { ProtecaoRotas } from "@/features/autenticacao/components/ProtecaoRotas";

type ThemeProviderProps = {
  children: React.ReactNode;
};

export function ThemeProvider({ children }: ThemeProviderProps) {
  return (
    <AppRouterCacheProvider>
      <MuiThemeProvider theme={theme}>
        <CssBaseline />
        <SessaoProvider>
          <ProtecaoRotas>{children}</ProtecaoRotas>
        </SessaoProvider>
      </MuiThemeProvider>
    </AppRouterCacheProvider>
  );
}
