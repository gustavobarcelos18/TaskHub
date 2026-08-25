"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import Button, { type ButtonProps } from "@mui/material/Button";
import CardActionArea, {
  type CardActionAreaProps,
} from "@mui/material/CardActionArea";
import IconButton, { type IconButtonProps } from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";

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
  tooltip?: ReactNode;
};

export function IconBotaoLink({
  href,
  children,
  tooltip,
  ...props
}: IconBotaoLinkProps) {
  const botao = (
    <IconButton component={Link} href={href} {...props}>
      {children}
    </IconButton>
  );

  return tooltip ? <Tooltip title={tooltip}>{botao}</Tooltip> : botao;
}

type AreaAcaoLinkProps = CardActionAreaProps & {
  href: string;
  children: ReactNode;
};

export function AreaAcaoLink({ href, children, ...props }: AreaAcaoLinkProps) {
  return (
    <CardActionArea component={Link} href={href} {...props}>
      {children}
    </CardActionArea>
  );
}
