# Architecture

## Approach

Clean/Onion Architecture, layered. Goal: low coupling, business logic independent from infrastructure (persistence, notifications, web framework). Recommended folder structure:

```
/src
  /backend
    /Api
    /Application
    /Domain
    /Infrastructure
    /Tests
  /frontend
    /src
      /components
      /pages
      /services
      /types
      /hooks
/database
  schema.sql
  seed.sql
/docs
  architecture.md
  conventions.md
  open-decisions.md
  /adr
```

## Layers

```
Api            → HTTP entrypoint. Controllers, middlewares, DI configuration, Swagger.
                  Contains no business logic.
Application    → use cases (application services), DTOs, validation,
                  interfaces that Infrastructure implements (INotificationService,
                  INotificationChannel, IRepository where applicable).
Domain         → entities (Solicitud, Usuario, HistorialEstado, Comentario,
                  Notificacion, Area, TipoSolicitud), enums (EstadoSolicitud,
                  Prioridad, RolUsuario), pure business rules. No external
                  dependencies (no EF Core, no ASP.NET).
Infrastructure → EF Core (DbContext, migrations, concrete repositories),
                  notification channel implementation, logging (Serilog).
```

Dependency rule: `Api → Application → Domain`, with `Infrastructure` implementing interfaces defined in `Application`/`Domain` (injected via DI in `Api`). `Domain` depends on no other layer.

> Naming note: entity, enum, and state names stay in Spanish in the code (`Solicitud`, `EstadoSolicitud`, `Resuelta`, `Cerrada`, etc.), per the naming rules in `docs/conventions.md`. This document is written in English but doesn't translate them.

## Main entities and relationships

| Entity            | Minimum fields                                                      | Relationships                         | Notes                                                                                                                              |
| ----------------- | ------------------------------------------------------------------- | ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `Usuario`         | Id, name, email, role, active                                       | Created/assigned requests, comments   | Can use Identity or a simplified custom model                                                                                      |
| `Solicitud`       | Id, code, title, description, priority, state, createdDate, dueDate | Requesting user, assignee, area, type | The core domain entity                                                                                                             |
| `Area`            | Id, name, active                                                    | Requests                              | Manageable catalog or seed data                                                                                                    |
| `TipoSolicitud`   | Id, name, description, active                                       | Requests                              | Classifies requests                                                                                                                |
| `HistorialEstado` | Id, previousState, newState, date, comment                          | Request, user                         | Basis for traceability and audit; includes the transition comment (see ADR-0001 for how the resolution comment is derived from it) |
| `Comentario`      | Id, text, visibility, date                                          | Request, user                         | Distinguishes internal vs. requester-visible comments                                                                              |
| `Notificacion`    | Id, channel, subject, message, status, date                         | Request, target user                  | A record/audit trail, not the delivery mechanism itself — see ADR-0003 (still proposed)                                            |

## State flow

See ADR-0002 for the sequential-transition rule leading into `Cerrada`.

## Expected extensibility

The solution must support adding new request types, states, or notification mechanisms without rewriting business logic. This is addressed with:

- Catalogs (`Area`, `TipoSolicitud`) as tables, not hardcoded enums.
