# Code Conventions

These rules are **an evaluation requirement** of the technical assessment (not a style preference) and are reproduced here in full — the client's original document is not included in this repository. An AI agent generating code in this repo must follow them always, without asking for confirmation.

## Naming (C# / backend)

| Element         | Convention | Example                |
| --------------- | ---------- | ---------------------- |
| Classes         | PascalCase | `SolicitudService`     |
| Methods         | PascalCase | `CambiarEstadoAsync`   |
| Enums           | PascalCase | `EstadoSolicitud`      |
| Properties      | PascalCase | `FechaCompromiso`      |
| Local variables | camelCase  | `solicitudActual`      |
| Parameters      | camelCase  | `solicitudId`          |
| Constants       | UPPERCASE  | `MAX_LONGITUD_TITULO`  |
| Interfaces      | `I` prefix | `INotificationChannel` |

> Note: identifier names themselves stay in Spanish (matching the domain — `Solicitud`, `EstadoSolicitud`, etc.), since that's how the original specification and the rest of the codebase name things. Only this document's prose is in English.

## Additional rules (mandatory)

- No abbreviations in names (`descripcion`, not `desc`; `solicitudId`, not `solId`).
- No magic numbers — extract them into named constants.
- Connection strings and sensitive parameters go in `appsettings.json` / environment variables, never hardcoded.

## Architecture and layers

- Backend project naming: `[SB].[NombreProyecto].[Capa]` (e.g. `SB.Solicitudes.Api`, `SB.Solicitudes.Domain`).
- Strict separation: `Domain` does not depend on `Infrastructure`; `Application` defines interfaces that `Infrastructure` implements (dependency inversion).

## Frontend (React + TypeScript)

- Components: PascalCase (`SolicitudCard.tsx`).
- Custom hooks: `use` prefix + camelCase (`useSolicitudes.ts`).
- Types/interfaces: PascalCase, no `I` prefix (different convention from C# — `Solicitud`, not `ISolicitud`).

## When generating new code

If a convention isn't covered here, follow the closest existing pattern already present in the codebase before improvising a new one. If there's no precedent, decide and log it in `docs/open-decisions.md` for confirmation.
