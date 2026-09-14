export function Footer() {
  return (
    <footer className="flex shrink-0 items-center justify-between border-t border-slate-200 bg-white px-8 py-3 text-xs text-slate-400">
      <span>© {new Date().getFullYear()} Portal de Solicitudes Internas</span>
      <span>Prueba técnica</span>
    </footer>
  );
}
