# ADR-0035: Pruebas unitarias de la capa de aplicación

**Estado:** Aceptada
**Fecha:** 2026-09-14

## Contexto

El requerimiento pide pruebas unitarias y/o de integración sobre los componentes críticos, y `docs/architecture.md` ya preveía una carpeta `/Tests`, pero la solución no tenía ningún proyecto de pruebas. La corrección de los huecos de autorización (ADR-0012, amendada) se verificó a mano contra la API; hacía falta que esa verificación quedara automatizada para que una regresión no volviera a pasar desapercibida.

Quedaban por decidir el nombre y la ubicación del proyecto, el framework de pruebas, la librería de dobles y qué cubrir primero.

## Decisión

Un proyecto `SB.PortalSolicitudes.UnitTests` (net8.0), hermano de los otros cuatro e incluido en `SB.PortalSolicitudes.slnx`. Referencia solo a `Application` (y por transitividad a `Domain`): prueba handlers y validadores **sin base de datos, sin HTTP y sin contenedor de dependencias**, sustituyendo `IUnitOfWork`, los repositorios, `IUsuarioActual`, `IProveedorFechaHora`, `IGeneradorCodigoSolicitud` e `INotificationService`. Es posible porque los handlers ya dependen solo de interfaces (ADR-0009).

- **Framework:** xUnit 2.9 con `Assert` propio. Es el estándar de facto en .NET y se integra con `dotnet test` sin configuración.
- **Dobles:** NSubstitute 5.3 (licencia BSD-3).
- **Sin librería de aserciones fluidas:** FluentAssertions pasó a licencia comercial desde la v8; `Assert` de xUnit alcanza para este volumen.
- **Nombres:** `Metodo_Escenario_ResultadoEsperado`, en español como el resto del código. Los identificadores de prueba son constantes con nombre en `Comun/DatosPrueba.cs` (docs/conventions.md: sin números mágicos) y reproducen el reparto de la semilla de demostración.
- **Estructura:** las carpetas espejan las features de `Application` (`Auth`, `Catalogos`, `Common`, `Dashboard`, `EntidadesGubernamentales`, `Notificaciones`, `Seed`, `Solicitudes`, `Usuarios`). Un archivo por handler cuando tiene reglas propias; un archivo por feature (`AreasHandlersTests`, `AuthHandlersTests`, …) cuando son varios handlers pequeños que comparten dobles.
- **Cobertura medida:** `coverlet.collector` (MIT), el colector estándar de `dotnet test`.

**Alcance: todos los handlers de `Application`** (39 de 39), más `AlcanceSolicitudes` y los validadores de solicitud. Cada handler tiene pruebas de su camino exitoso y de cada rama de error que distingue: no encontrado, conflicto, prohibido, sin sesión y falla cerrada del alcance. Lo que cada grupo fija:

| Grupo | Qué fija |
| --- | --- |
| `AlcanceSolicitudes` | Visibilidad por rol y falla cerrada (ADR-0012, ADR-0029) |
| Solicitudes (10 handlers) | Máquina de estados (transición no declarada, reapertura solo personal, comentario obligatorio, reversión ante fallo), asignación, edición parcial solo en `REGISTRADA`, edición completa con comentario de auditoría, comentarios internos, evidencias, detalle, transiciones disponibles y el filtro de listado que el cliente no puede ampliar; alcance con 404 en todos |
| Dashboard | Conteos con cero para estados/prioridades sin solicitudes, vencidas contra la hora del proveedor, mismo alcance que el listado |
| Auth (4) | Mismo error para correo inexistente y contraseña incorrecta; usuario desactivado no inicia sesión ni renueva; la renovación usa el usuario de la base, no los claims viejos; auto-registro siempre como Solicitante |
| Usuarios (3) | "Administrador no modifica Administrador" con su excepción propia; edición parcial; listado por rol sin paginar |
| Catálogos (13) y Entidades gubernamentales (5) | Nombre único excluyendo el propio registro al editar, alta activa, edición parcial, activas vs. todas |
| Notificaciones (2) | Cada usuario solo ve las suyas; sin Id falla cerrada |
| Seed | Bloqueado si no está habilitado, migraciones antes de los datos, idempotente, y los datos de demostración respetan las reglas de ADR-0001/0002 |

Resultado al cierre de este ADR: **177 pruebas, 100 % de líneas y 96,8 % de ramas** en los handlers. Las ramas sin cubrir son los operadores `??` de las ediciones parciales cuando un campo concreto viene informado.

Dos defectos salieron al escribir las pruebas; para cada uno se escribió primero la prueba que fallaba y después la corrección:

- **Huecos de autorización por `Id`** (ADR-0012, amendada): las pruebas de regresión fallan contra el código anterior a la corrección (commit `1cd9b4f`) y pasan con ella.
- **`ObtenerNotificacionesPaginadoQueryHandler` no fallaba cerrado:** con `IUsuarioActual.Id` nulo, el filtro `UsuarioDestinoId` quedaba en `null`, que el repositorio interpreta como "sin filtro", así que devolvía las notificaciones de todos los usuarios. Ahora responde `Auth.NoAutenticado`, igual que `ObtenerResumenNotificacionesQueryHandler` y con el criterio de ADR-0029. Como en ese ADR, solo es alcanzable con un JWT firmado cuyo claim de Id no se puede leer.

Pendiente detectado, no corregido: `ObtenerUsuariosPaginadoQueryHandler` con `Rol` devuelve la lista completa, pero `ResultadoPaginado` limita `TamanoPagina` a 100. Con más de 100 usuarios de un rol, `TotalPaginas` sería mayor que 1 aunque todos los elementos lleguen en la primera página.

Ejecución: `dotnet test` sobre `src/backend/SB.PortalSolicitudes/SB.PortalSolicitudes.slnx`, o sobre el `.csproj` del proyecto de pruebas si solo se cuenta con el SDK de .NET 8 (el formato `.slnx` requiere un SDK más nuevo). Cobertura: `dotnet test --collect:"XPlat Code Coverage"`, que deja un `coverage.cobertura.xml` en `TestResults/`.

## Consecuencias

- **A favor:** las reglas de negocio y de autorización de todos los handlers quedan fijadas en pruebas que corren en menos de un segundo, sin infraestructura. Una regresión como la de ADR-0012 falla en `dotnet test` en vez de descubrirse contra la API.
- **Límite de la métrica:** el 100 % de líneas dice que cada línea de un handler se ejecuta en alguna prueba, no que cada comportamiento esté bien especificado. Por eso las pruebas afirman efectos concretos (qué se guardó, qué no se guardó, a quién se notificó), no solo que el resultado sea exitoso.
- **En contra / trade-offs:** al simular los repositorios, estas pruebas no cubren las consultas LINQ reales (filtros, paginación, `SolicitudRepository.AplicarAlcance`), la generación concurrente del código en SQL Server ni la cadena HTTP completa (autenticación, `[Authorize]`, traducción de `Resultado` a códigos HTTP). Además, `AlcanceSolicitudes.Incluye` y `SolicitudRepository.AplicarAlcance` expresan la misma regla dos veces, y solo la primera queda probada aquí.
- **Siguiente paso previsto:** un proyecto separado `SB.PortalSolicitudes.IntegrationTests` (`WebApplicationFactory` y SQL Server de pruebas) para esos huecos. Va aparte porque sus dependencias y su tiempo de ejecución son distintos, y así las pruebas unitarias siguen siendo rápidas.
- **Alternativas descartadas:** Moq, por la controversia de SponsorLink (telemetría incluida en una versión menor); EF Core InMemory en lugar de dobles, porque no se comporta como SQL Server (sin transacciones reales ni el SQL crudo del generador de código) y daría una falsa sensación de cobertura de persistencia.
