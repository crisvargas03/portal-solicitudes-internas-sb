import type { ButtonHTMLAttributes } from 'react';
import type { LucideIcon } from 'lucide-react';
import { Loader2 } from 'lucide-react';
import { buttonClasses } from './buttonClasses';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: 'primario' | 'secundario' | 'fantasma' | 'peligro';
  tamano?: 'sm' | 'md';
  icono?: LucideIcon;
  /** Deshabilita el botón y muestra un spinner en vez del ícono, para el pending de un submit. */
  cargando?: boolean;
}

export function Button({
  variante = 'primario',
  tamano = 'md',
  icono: Icono,
  cargando = false,
  disabled,
  className,
  children,
  ...resto
}: ButtonProps) {
  return (
    <button
      className={`${buttonClasses(variante, tamano)} disabled:cursor-not-allowed disabled:opacity-60 ${className ?? ''}`}
      disabled={disabled || cargando}
      {...resto}
    >
      {cargando ? <Loader2 size={16} className="animate-spin" /> : Icono && <Icono size={16} />}
      {children}
    </button>
  );
}
