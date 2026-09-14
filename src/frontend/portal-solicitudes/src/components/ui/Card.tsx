import type { PropsWithChildren, ReactNode } from 'react';

interface CardProps extends PropsWithChildren {
  titulo?: string;
  accion?: ReactNode;
  className?: string;
}

/** Contenedor genérico de sección. Reemplaza al antiguo ContentCard, ahora con encabezado opcional. */
export function Card({ titulo, accion, children, className }: CardProps) {
  return (
    <div className={`rounded-xl bg-white p-6 shadow-sm sm:p-8 ${className ?? ''}`}>
      {(titulo || accion) && (
        <div className="mb-4 flex items-center justify-between gap-3">
          {titulo && <h2 className="text-sm font-semibold text-navy">{titulo}</h2>}
          {accion}
        </div>
      )}
      {children}
    </div>
  );
}
