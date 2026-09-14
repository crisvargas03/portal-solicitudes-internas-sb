# Portal de Solicitudes Internas

Prueba técnica — Plataforma interna tipo *helpdesk* para que los empleados de la
Superintendencia de Bancos (SB) registren, den seguimiento y resuelvan solicitudes de
soporte/tecnología, con historial de estados, notificaciones, tablero de métricas y un
mantenimiento aparte de entidades gubernamentales de RD.

- Backend: .NET 8 / ASP.NET Core Web API, EF Core, SQL Server.
- Frontend: React 19 + TypeScript, Zustand, React Query, Zod, Tailwind.
- Arquitectura: Clean/Onion por capas (`Api → Application → Domain`, `Infraestructure`
  implementando interfaces de `Application`). Detalle completo en
  [`docs/architecture.md`](docs/architecture.md) y en las [ADR](docs/ADR).

## Requisitos

- [Docker](https://www.docker.com/) (Desktop o Engine) con `docker compose`.
- [Node.js](https://nodejs.org/) 20+ y [pnpm](https://pnpm.io/) para el frontend.
- Opcional, solo si no vas a usar Docker: .NET SDK 8 para correr la API en el host.

## Levantar el proyecto

### 1. Backend + base de datos (Docker)

```bash
docker compose up -d --build
```

Levanta dos contenedores: `mssql` (SQL Server 2022, con volumen `mssql-datos` y
healthcheck) y `api` (build multi-etapa del `Dockerfile`, espera a que la base esté
saludable). La API queda expuesta en `http://localhost:5008`, con Swagger en
`http://localhost:5008/swagger`.

**La base arranca vacía a propósito** (ver [ADR-0008](docs/ADR/ADR-0008-persistencia-migraciones-y-ejecucion.md)):
la API no migra al iniciar. Antes de usar el portal, prepárala con un solo endpoint:

```
GET http://localhost:5008/api/seed
```

Llámalo una vez desde Swagger (o `curl http://localhost:5008/api/seed`). Es idempotente
(se puede volver a llamar sin duplicar datos) y aplica las migraciones pendientes, luego
carga usuarios y solicitudes de demostración. Solo está habilitado en `Development` o con
`Seed:Habilitado=true` — no puede ejecutarse contra un despliegue real por accidente.

Para detener todo: `docker compose down` (agrega `-v` si además quieres borrar el volumen
de SQL Server y volver a empezar desde cero).

> Alternativa sin Docker para la API: `dotnet run --project src/backend/SB.PortalSolicitudes/SB.PortalSolicitudes.API`
> (necesita SQL Server accesible y la cadena de conexión en `appsettings.Development.json`
> apuntando a él). Queda en `http://localhost:5028`, con Swagger en `/swagger`.

### 2. Frontend

```bash
cd src/frontend/portal-solicitudes
pnpm install
cp .env.example .env   # ya trae http://localhost:5008/api (Docker); comenta/descomenta según cómo corras la API
pnpm dev
```

Queda en `http://localhost:5173`.

### Usuarios de prueba

Los crea `GET /api/seed`, uno por rol (y dos analistas/solicitantes para poder probar
asignación y alcance por usuario):

| Rol | Email | Password |
|---|---|---|
| Administrador | `admin@portalsolicitudes.test` | `Admin123!` |
| Analista | `analista1@portalsolicitudes.test` | `Analista123!` |
| Analista | `analista2@portalsolicitudes.test` | `Analista123!` |
| Solicitante | `solicitante1@portalsolicitudes.test` | `Solicitante123!` |
| Solicitante | `solicitante2@portalsolicitudes.test` | `Solicitante123!` |

El seed también crea 5 solicitudes de ejemplo repartidas en distintos estados
(`Registrada`, `En análisis`, `En progreso`, `Resuelta`, `Cerrada`) para poder ver el
flujo completo sin capturar datos a mano.

## Correr las pruebas

```bash
dotnet test src/backend/SB.PortalSolicitudes/SB.PortalSolicitudes.slnx
```

> El `.slnx` requiere SDK 9.0.200+ para abrirlo/restaurarlo desde la CLI. Con solo el SDK
> 8 instalado (el mismo que usa el `Dockerfile`), corre los comandos apuntando al proyecto
> de la API o de tests directamente, p. ej.
> `dotnet test src/backend/SB.PortalSolicitudes/SB.PortalSolicitudes.UnitTests`.

Suite de unitarias sobre la capa `Application` (handlers de Solicitudes, Usuarios,
Catálogos, Auth, Dashboard, Notificaciones, Entidades Gubernamentales y Seed) — ver
[ADR-0035](docs/ADR/ADR-0035-pruebas-unitarias-de-la-capa-de-aplicacion.md). Cubre reglas
de negocio (transiciones de estado válidas, comentario obligatorio al cerrar, alcance de
autorización por rol) sin depender de una base de datos real.

## Roles y autorización

| Rol | Alcance |
|---|---|
| **Administrador** | Acceso completo: solicitudes, catálogos (áreas, prioridades, tipos, entidades gubernamentales) y usuarios. |
| **Analista** | Solicitudes asignadas a él o sin asignar; puede tomar/reasignar, cambiar estado, comentar y reabrir. |
| **Solicitante** | Solo sus propias solicitudes: crearlas, verlas y comentarlas. |

Las reglas se validan en el backend (no solo se ocultan botones en el frontend): cada
handler que opera sobre una solicitud existente aplica el mismo criterio de alcance
(`AlcanceSolicitudes`, ver [ADR-0012](docs/ADR/ADR-0012-alcance-de-autorizacion-por-rol.md)),
así que un Solicitante o Analista no puede operar una solicitud fuera de su alcance
llamando directamente a la API.

## Mantenimiento de Entidades Gubernamentales

Módulo aparte, con su propia fuente de datos: un archivo de texto plano
(`Data/entidades-gubernamentales.txt`) dentro del proyecto de la API, en vez de la base
SQL Server que usa el resto de la solución — así lo pide la especificación de ese módulo.
Detalle en [ADR-0034](docs/ADR/ADR-0034-persistencia-en-archivo-de-entidades-gubernamentales.md).
Datos de origen: `docs/ListaEntidadesGubernamentales.xlsx` (181 registros). CRUD completo
(alta, edición, activar/desactivar) accesible desde `/entidades-gubernamentales` en el
frontend (solo Administrador), no solo por Swagger.

## Estructura del repositorio

```
/src
  /backend/SB.PortalSolicitudes
    /SB.PortalSolicitudes.API             → controllers, DI, Swagger, middlewares
    /SB.PortalSolicitudes.Application     → handlers (LiteBus), DTOs, validación, Result pattern
    /SB.PortalSolicitudes.Domain          → entidades y enums, sin dependencias externas
    /SB.PortalSolicitudes.Infraestructure → EF Core, repositorios, notificaciones, Serilog
    /SB.PortalSolicitudes.UnitTests
  /frontend/portal-solicitudes            → React + TS (components, pages, services, hooks, types)
/docs
  architecture.md      → capas, entidades, flujo de estados
  conventions.md        → convenciones de nombres exigidas por la prueba
  open-decisions.md     → decisiones no explícitas en la especificación, registradas antes de asumirlas
  CLAUDE.md              → contexto completo de la prueba (alcance, modelo mínimo, endpoints sugeridos)
  /ADR                   → 36 decisiones de arquitectura documentadas, numeradas y con estado
docker-compose.yml
README.md
```

No hay `database/schema.sql`/`seed.sql` escritos a mano: las migraciones de EF Core son
la única fuente de verdad del esquema (razón completa en
[ADR-0008](docs/ADR/ADR-0008-persistencia-migraciones-y-ejecucion.md)); para inspeccionar
el SQL generado, `dotnet ef migrations script` desde `SB.PortalSolicitudes.Infraestructure`.

## Decisiones técnicas destacadas

- **Result pattern + contrato único de respuesta**: los handlers devuelven `Resultado<T>`;
  la API traduce eso a un envelope de éxito o a `ProblemDetails` de forma consistente en
  todos los endpoints ([ADR-0010](docs/ADR/ADR-0010-resultado-y-contrato-de-respuesta.md)).
- **Handlers en vez de una capa de casos de uso aparte**: la lógica de negocio vive
  directamente en los command/query handlers de `Application` (LiteBus como mediador,
  [ADR-0014](docs/ADR/ADR-0014-litebus-como-mediador.md)), no en un `IUseCase` adicional.
- **Catálogos y transiciones como datos, no enums/`switch`**: `EstadoSolicitud`,
  `Prioridad` y `TransicionPermitida` son tablas ([ADR-0004](docs/ADR/ADR-0004-catalogos-en-tabla-en-lugar-de-enumeraciones.md),
  [ADR-0005](docs/ADR/ADR-0005-transiciones-dirigidas-por-datos.md)), así que agregar un
  estado o una transición nueva es una fila, no un despliegue de código.
- **Notificaciones desacopladas**: `INotificationService` orquesta canales
  (`INotificationChannel`) sin que el resto del dominio conozca el mecanismo de entrega —
  se puede cambiar o agregar un canal sin tocar la lógica de solicitudes.
- Índice completo de decisiones, con contexto y alternativas descartadas, en
  [`docs/ADR`](docs/ADR).

## Notas y limitaciones conocidas

- Las evidencias de una solicitud son referencia textual o URL — no hay carga real de
  archivos, por especificación.
- `docker-compose.yml` fija credenciales y clave JWT de desarrollo directamente (no son
  secretos reales); en un despliegue real vendrían de variables de entorno o un vault.
- `appsettings.Development.json` está versionado con valores de desarrollo (clave JWT y
  cadena de conexión no sensibles) a propósito, para que el proyecto sea reproducible sin
  configuración adicional; `appsettings.*.local.json`/`secrets.json` quedan ignorados por
  git para cualquier valor real.
