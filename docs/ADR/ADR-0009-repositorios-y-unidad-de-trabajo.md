# ADR-0009: Repositorios + Unidad de Trabajo reemplazan a `IPortalSolicitudesDbContext`

**Estado:** Aceptada
**Fecha:** 2026-09-10

## Contexto

`Application/Abstractions/IPortalSolicitudesDbContext.cs` era, hasta ahora, la unica abstraccion de persistencia: un `DbSet<T>` por entidad y `SaveChangesAsync`. Funcionaba mientras `Application` no tenia consumidores, pero los handlers de MediatR que siguen (listado paginado y filtrado de `Solicitud`, cambio de estado que actualiza `Solicitud` y agrega un `HistorialEstado` en la misma operacion, metricas del dashboard) necesitan mas que un passthrough:

- una forma de expresar "carga una solicitud con su historial" sin repetir el `Include` en cada handler;
- una unidad que agrupe varias escrituras (estado + historial + notificacion) en una sola transaccion;
- una superficie de prueba: sustituir la persistencia por un doble de prueba sin arrancar SQL Server.

`IPortalSolicitudesDbContext` no resuelve nada de esto por si solo: cada handler terminaria escribiendo su propio LINQ contra `DbSet<T>`, con EF Core como dependencia directa de `Application` (el `PackageReference` a `Microsoft.EntityFrameworkCore` ya estaba ahi para sostener `DbSet<T>`).

## Decision

Se reemplaza `IPortalSolicitudesDbContext` por repositorios especificos por entidad (`IUsuarioRepository`, `ISolicitudRepository`, etc., mas `ICatalogoRepository<TCatalogo>` compartido por `Area`, `TipoSolicitud`, `Prioridad` y `EstadoSolicitud`) coordinados por `IUnitOfWork`, todos declarados en `Application/Abstractions/Persistence` e implementados en `Infraestructure/Persistence`. Los handlers dependen unicamente de `IUnitOfWork`; no conocen `DbSet<T>` ni `DbContext`.

Los listados filtrados y paginados (`ISolicitudRepository.ObtenerPaginadoAsync`, `INotificacionRepository.ObtenerPaginadoAsync`) reciben un objeto de filtro (`FiltroSolicitudes`, `FiltroNotificaciones`, ambos derivados de `ParametrosPaginacion`) y devuelven `ResultadoPaginado<T>`. Ningun `IQueryable` sale de `Infraestructure`.

Como consecuencia directa, el `PackageReference` a `Microsoft.EntityFrameworkCore` se retira de `SB.PortalSolicitudes.Application.csproj`: sin `IPortalSolicitudesDbContext`, la capa no tiene ninguna razon para referenciar EF Core, y su ausencia hace que el compilador imponga la regla de dependencia (`docs/architecture.md`), no solo la convencion.

## Consecuencias

- **A favor:** los handlers llaman operaciones con nombre e intencion (`ObtenerParaCambioDeEstadoAsync`, `ObtenerDetalleAsync`) en lugar de componer consultas ad-hoc; `IUnitOfWork.IniciarTransaccionAsync`/`ConfirmarTransaccionAsync` da un punto explicito para las operaciones que tocan mas de una tabla; `Application` queda sin ninguna referencia a EF Core, verificable con `dotnet build`; los repositorios son la unidad natural para pruebas de integracion futuras.
- **En contra / trade-offs:** una capa mas sobre un `DbContext` que ya era abstracto — para un proyecto de este tamano el beneficio esta en el desacople de EF Core y en nombrar las consultas, no en cambiar de motor de base de datos; agregar una consulta nueva implica tocar la interfaz y la implementacion; los repositorios de catalogo comparten una base pero cada catalogo sigue teniendo su propia interfaz nombrada, lo que agrega archivos aunque casi todos queden vacios.
- **Alternativas descartadas:** mantener `IPortalSolicitudesDbContext` junto a los repositorios (unos para lectura ad-hoc, otros para escritura), descartada por introducir dos caminos hacia la misma base de datos sin una regla clara de cuando usar cada uno; exponer `IQueryable<T>` desde los repositorios, descartada porque filtra la semantica de ejecucion diferida de EF Core a `Application` y facilita evaluacion en cliente por accidente.
