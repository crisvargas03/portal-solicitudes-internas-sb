# ADR-0008: Persistencia con migraciones de EF Core, Docker y endpoint de datos de prueba

**Estado:** Aceptada
**Fecha:** 2026-09-09

## Contexto

`docs/CLAUDE.md` deja la pregunta abierta de forma literal, en el bloque de comandos: *"(completar cuando se decida: script .sql o migraciones de EF)"*, y la estructura propuesta reserva `database/schema.sql` y `database/seed.sql` escritos a mano. Con dos fuentes posibles de verdad para el esquema, había que elegir una.

A esto se suma un requisito práctico de la prueba técnica: quien la evalúa debe poder levantar la solución y ver datos reales sin instalar SQL Server ni el SDK de .NET, y sin ejecutar una secuencia de comandos frágil.

Las opciones para el esquema eran mantener `schema.sql` a mano (y sincronizarlo con el modelo manualmente), o usar migraciones de EF Core como única fuente. Para los datos, mezclarlos todos en la migración, o separarlos según su naturaleza.

## Decisión

**Esquema.** Las migraciones de EF Core son la única fuente de verdad. `database/schema.sql` no se escribe a mano; si se necesita un script para revisión se genera con `dotnet ef migrations script`. Las entidades se mapean en `Infraestructure` con `IEntityTypeConfiguration<T>`, no con anotaciones, para que `Domain` siga sin paquetes. El motor es SQL Server (`mcr.microsoft.com/mssql/server:2022-latest`), y la cadena de conexión se lee de configuración o variables de entorno, nunca del código (`docs/conventions.md`).

**Datos, en dos mecanismos según su naturaleza.**

- *Catálogos estructurales* — `Prioridad`, `EstadoSolicitud`, `TransicionPermitida` y sus roles, `Area`, `TipoSolicitud` — viajan en la migración vía `HasData`. Sin ellos la aplicación no es válida: la máquina de estados de [ADR-0005](ADR-0005-transiciones-dirigidas-por-datos.md) *es* la fila de `TransicionPermitida`, así que forma parte del esquema, no de los datos de prueba.
- *Datos de demostración* — usuarios con contraseña conocida, solicitudes repartidas por los distintos estados, historial, comentarios, adjuntos y notificaciones — **no** van en la migración. Se cargan a demanda mediante un endpoint de preparación.

**Endpoint de preparación.** `POST /api/setup/seed` carga el juego de datos de demostración, `GET /api/setup/estado` informa si ya está cargado y `POST /api/setup/reset` lo reinicia. Es idempotente y está **restringido**: se habilita solo en entorno `Development` o con una bandera explícita de configuración (`Seed:Habilitado`), de modo que no pueda ejecutarse contra un despliegue real.

**Ejecución.** `docker-compose.yml` con dos servicios: `mssql` (volumen con nombre para persistir, con *healthcheck*) y `api` (Dockerfile multi-etapa, espera el *healthcheck* de la base y aplica las migraciones al arrancar). El frontend se ejecuta en el host con `pnpm dev` apuntando a la API del contenedor.

## Consecuencias

- **A favor:** una sola fuente de verdad para el esquema, imposible de desincronizar del modelo; el historial de cambios de base de datos queda versionado y revisable en el repositorio; quien evalúa levanta todo con `docker compose up` sin instalar SQL Server ni el SDK, y obtiene un portal con datos realistas llamando a un endpoint desde Swagger, en lugar de ejecutar scripts; la separación catálogos/demo permite desplegar sin datos ficticios pero nunca sin la máquina de estados.
- **En contra / trade-offs:** se introduce un endpoint que modifica datos y que hay que mantener deliberadamente cercado — si la bandera se habilitara por descuido en un entorno real, sería un endpoint destructivo expuesto; aplicar migraciones automáticamente al arrancar es cómodo para una prueba técnica pero es una práctica que en producción se sustituye por un paso de despliegue controlado; revisar el esquema exige leer C# o generar el script, en lugar de abrir un `.sql`; y el arranque depende de Docker y de que el contenedor de SQL Server esté saludable antes que la API.
- **Alternativas descartadas:** mantener `schema.sql` y `seed.sql` escritos a mano, descartado por el costo de sincronizarlos con el modelo a cada cambio y por el riesgo de divergencia silenciosa; poner también los datos de demostración en la migración vía `HasData`, descartado porque mezcla datos ficticios con el esquema y los volvería imposibles de omitir en un despliegue; y exigir que quien evalúa ejecute `dotnet ef database update`, descartado por requerir el SDK de .NET y la herramienta `dotnet-ef` instalados en su máquina.
