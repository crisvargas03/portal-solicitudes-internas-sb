# ADR-0017: Validación de formularios con Zod + React Hook Form

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

No existía ninguna librería de validación de formularios en el frontend, ni componentes de input reutilizables: `SolicitudForm` era markup no controlado sin estado ni manejador de submit, y `Login` recogía email/password sin validarlos. El backend ya valida con FluentValidation (ver ADR-0013) y devuelve errores de campo en PascalCase dentro de `error.errores` en un 400 — el formulario de login debía reflejar eso.

También faltaba un token de color para estados de error: la app solo tenía `red-*` de Tailwind sueltos, sin relación con la paleta semántica (`components/ui/tono.ts`) que ya usa el resto de la UI para colorear por significado.

## Decisión

**Zod + React Hook Form + `@hookform/resolvers`.** Esquemas en `/src/schemas`, un archivo por formulario/entidad (`loginSchema.ts` por ahora). `useForm({ resolver: zodResolver(schema), mode: 'onTouched' })`: los errores solo aparecen después de que el campo fue tocado o se intentó enviar, nunca antes.

El schema de login exige email válido (el backend valida `NotEmpty().EmailAddress()`, no username) y contraseña no vacía, sin `min()` de longitud — una regla de longitud en cliente rechazaría credenciales válidas que ya existen en la base de datos.

**Token de color:** `--color-danger: #b91c1c` (= `red-700`, el mismo rojo que ya usa el tono `critico` de `tono.ts`, para no tener dos rojos casi iguales en la app) y `--color-danger-soft: #fee2e2` para fondos de aviso, ambos en el bloque `@theme` de `src/index.css` (Tailwind v4, config CSS-first).

**Componentes nuevos:**

- `components/ui/inputClasses.ts`: centraliza la clase canónica de input (antes copiada en cuatro archivos) y agrega el estado de error (`border-danger ring-1 ring-danger`).
- `components/ui/FormField.tsx`: label + control + mensaje de error inline (`text-xs text-danger`) debajo, con `aria-invalid`/`aria-describedby`. Props en español (`etiqueta`, `error`), como el resto de `/ui`.
- `components/ui/Button.tsx` ganó un prop `cargando` (antes no tenía estado de pending), usado en el submit del login en vez de un botón hecho a mano.

Las longitudes máximas para el futuro formulario de Solicitud ya están registradas en `docs/open-decisions.md` (título 150, descripción 2000, comentario 1000) — este ADR no las repite, solo deja el mecanismo (Zod + FormField) listo para usarlas cuando ese formulario se conecte.

## Consecuencias

- **A favor:** un solo patrón de validación para todos los formularios futuros; el mapeo de errores 400 del backend a campos del form es directo porque `ErrorApi.errores` ya llega con las claves de FluentValidation; el rojo de error es el mismo que ya significa "crítico" en el resto de la app, no un color nuevo que competir visualmente con el naranja de acento.
- **En contra / trade-offs:** dos fuentes de verdad para las reglas de un campo (Zod en cliente, FluentValidation en servidor) que hay que mantener alineadas a mano; `FormField` asume que su único hijo es un input/select/textarea controlado por `register`, no compone bien con inputs más complejos sin ajustes.
- **Alternativas descartadas:** validación hecha a mano con `useState` + funciones de validación (es lo que ya existía como ausencia de patrón — no escala a un segundo formulario); Yup en vez de Zod (Zod tiene mejor inferencia de tipos con TypeScript, que es como está tipado el resto del proyecto).
