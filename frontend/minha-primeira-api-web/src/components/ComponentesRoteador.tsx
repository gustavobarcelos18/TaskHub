"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import Button, { type ButtonProps } from "@mui/material/Button";
import IconButton, { type IconButtonProps } from "@mui/material/IconButton";

type BotaoLinkProps = ButtonProps & {
  href: string;
  children: ReactNode;
};

export function BotaoLink({ href, children, ...props }: BotaoLinkProps) {
  return (
    <Button component={Link} href={href} {...props}>
      {children}
    </Button>
  );
}

type IconBotaoLinkProps = IconButtonProps & {
  href: string;
  children: ReactNode;
};

export function IconBotaoLink({
  href,
  children,
  ...props
}: IconBotaoLinkProps) {
  return (
    <IconButton component={Link} href={href} {...props}>
      {children}
    </IconButton>
  );
}