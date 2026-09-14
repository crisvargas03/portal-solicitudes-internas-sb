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
  (generated output only — the schema lives in EF Core migrations, see ADR-0008)
/docs
  architecture.md
  conventions.md
  open-decisions.md
  /ADR
```

## Layers

```
Api            → HTTP entrypoint. Controllers, middlewares, DI configuration, Swagger,
                  the RespuestaApi<T> success envelope and the Resultado<T> → ProblemDetails
                  mapping (see ADR-0010). Contains no business logic.
Application    → LiteBus command/query handlers (business logic lives here directly —
                  no separate application-service/use-case layer), DTOs, validation,
                  the Resultado/Error result type (ADR-0010), and interfaces that
                  Infrastructure implements: IUnitOfWork and one repository interface per
                  entity under Abstractions/Persistence (ADR-0009), plus
                  INotificationService/INotificationChannel once ADR-0003 is accepted.
Domain         → entities (Solicitud, Usuario, HistorialEstado, Comentario,
                  Notificacion, Adjunto, Area, TipoSolicitud, Prioridad,
                  EstadoSolicitud, TransicionPermitida) and enums (RolUsuario,
                  CanalNotificacion, EstadoNotificacion) — plain data
                  definitions with no behavior and no validation: business
                  rules and validation live in Application.
                  No external dependencies (no EF Core, no ASP.NET).
Infrastructure → EF Core (DbContext, IEntityTypeConfiguration mappings, migrations),
                  the concrete repositories and UnitOfWork implementing the
                  Application/Abstractions/Persistence interfaces (ADR-0009),
                  notification channel implementation, logging (Serilog).
```

Dependency rule: `Api → Application → Domain`, with `Infrastructure` implementing interfaces defined in `Application`/`Domain` (injected via DI in `Api`). `Domain` depends on no other layer. `Application` has no dependency on EF Core or any other Infrastructure concern — repository interfaces and `IUnitOfWork` are the only seam.

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

Beyond the documented minimum, the model adds:

| Entity                   | Fields                                                       | Relationships             | Notes                                                                                      |
| ------------------------ | ------------------------------------------------------------ | ------------------------- | ------------------------------------------------------------------------------------------ |
| `Prioridad`              | Id, name, `Nivel`, active                                    | Requests                  | Catalog table rather than an enum — see ADR-0004                                           |
| `EstadoSolicitud`        | Id, `Codigo`, name, `Orden`, `EsFinal`, active               | Requests, history         | Catalog table rather than an enum — see ADR-0004. `Codigo` is the stable key, never the Id |
| `TransicionPermitida`    | Id, origin state, target state, `RequiereComentario`, active | Two states, allowed roles | The state machine as data — see ADR-0005                                                   |
| `TransicionPermitidaRol` | Id, transition, role                                         | `TransicionPermitida`     | Roles authorized to run a transition; a child table, not a `[Flags]` enum                  |
| `Adjunto`                | Id, description, url, date                                   | Request, user             | Text reference / evidence URL only, no file storage — see ADR-0006                         |

`Usuario` additionally carries `PasswordHash` (simplified custom model, not ASP.NET Identity — see ADR-0007). Per ADR-0001, `Solicitud` has **no** resolution-comment field.

## State flow

See ADR-0002 for the sequential-transition rule leading into `Cerrada`, and ADR-0005 for how that rule (plus the mandatory resolution comment and the reopen-by-role restriction) is expressed as `TransicionPermitida` rows instead of compiled logic.

## Expected extensibility

The solution must support adding new request types, states, or notification mechanisms without rewriting business logic. This is addressed with:

- Catalogs (`Area`, `TipoSolicitud`, and per ADR-0004 also `Prioridad` and `EstadoSolicitud`) as tables, not hardcoded enums.
- Allowed state transitions declared as rows in `TransicionPermitida` (ADR-0005), so a new state or path is an insert, not a code change.
- Notification channels behind `INotificationChannel`, so the delivery mechanism can change without touching the domain.

## Persistence

EF Core migrations are the single source of truth for the schema; `database/schema.sql` is not hand-written. Structural catalogs ship inside the migration via `HasData`, while demo data is loaded on demand through a gated setup endpoint. See ADR-0008 for the full rationale and for how the solution is run with Docker.
