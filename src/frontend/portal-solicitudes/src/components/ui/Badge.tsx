import type { ReactNode } from 'react';
import { TONO_CLASES } from './tono';
import type { Tono } from './tono';

interface BadgeProps {
  tono: Tono;
  variante?: 'suave' | 'solido';
  children: ReactNode;
  className?: string;
}

export function Badge({ tono, variante = 'suave', children, className }: BadgeProps) {
  const clases = TONO_CLASES[tono][variante];
  return (
    <span
      className={`inline-flex w-fit items-center whitespace-nowrap rounded-full px-2.5 py-1 text-[11px] font-semibold ${clases} ${className ?? ''}`}
    >
      {children}
    </span>
  );
}
