import type { ButtonHTMLAttributes } from 'react';
import type { LucideIcon } from 'lucide-react';
import { buttonClasses } from './buttonClasses';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: 'primario' | 'secundario' | 'fantasma';
  tamano?: 'sm' | 'md';
  icono?: LucideIcon;
}

export function Button({ variante = 'primario', tamano = 'md', icono: Icono, className, children, ...resto }: ButtonProps) {
  return (
    <button className={`${buttonClasses(variante, tamano)} ${className ?? ''}`} {...resto}>
      {Icono && <Icono size={16} />}
      {children}
    </button>
  );
}
