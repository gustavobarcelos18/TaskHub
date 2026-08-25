import type { Metadata } from "next";
import "./globals.css";
import { ThemeProvider } from "@/theme/ThemeProvider";

export const metadata: Metadata = {
  title: "Gerenciador de Tarefas",
  description: "Sistema de gerenciamento de tarefas.",
};

type RootLayoutProps = Readonly<{
  children: React.ReactNode;
}>;

export default function RootLayout({ children }: RootLayoutProps) {
  return (
    <html lang="pt-BR">
      <body>
        <ThemeProvider>{children}</ThemeProvider>
      </body>
    </html>
  );
}