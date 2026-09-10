# Decisiones abiertas

Registro de lo que el requerimiento no especifica y todavía no se ha decidido. Cuando una entrada se resuelve, se convierte en un ADR en `docs/ADR/` y se retira de esta lista.

Referenciado por `docs/CLAUDE.md` ("si estás a punto de decidir algo que no es explícito, no lo decidas en silencio: márcalo y regístralo aquí") y por `docs/conventions.md`.

---

## 1. Canal de notificaciones (ADR-0003)

**Estado:** propuesta sin aceptar.

El requerimiento admite registro en base de datos, consola, correo simulado o evento en cola, y deja la implementación abierta. La propuesta es `INotificationService` + `INotificationChannel` desacoplados, con la entidad `Notificacion` como registro de auditoría y no como mecanismo de entrega.

Ya decidido y construido: la entidad `Notificacion` y las enumeraciones `CanalNotificacion` y `EstadoNotificacion` existen en `Domain`, sin comportamiento de envío.

Pendiente: qué canal se implementa por defecto, si se implementa más de uno, y si el envío es sincrónico o diferido. **No implementar envío real de correo ni de cola** — el alcance es simulado.

## 2. Generación del código `SOL-2026-0001`

**Estado:** abierta.

El formato está fijado por el requerimiento, la estrategia no. Opciones:

- secuencia de SQL Server por año, con el año tomado de la fecha de creación;
- tabla contadora con bloqueo, o columna calculada a partir del `Id`;
- consulta del máximo actual del año — descartable de entrada por condiciones de carrera.

La decisión debe indicar explícitamente cómo se comporta ante creaciones concurrentes y qué pasa al cambiar de año. Afecta a `Solicitud.Codigo`, que hoy solo valida formato y longitud (`MAX_LONGITUD_CODIGO = 20`) sin generar nada.

## 3. Longitudes máximas de los campos de texto

**Estado:** valores provisionales, pendientes de confirmación.

El requerimiento exige "validación de campos obligatorios y longitud máxima de texto" pero no da cifras.

Las entidades de `Domain` **no validan**: solo declaran las constantes. La comprobación de campos obligatorios, longitudes y formatos vive en la capa `Application` (validadores de DTO), y las mismas constantes alimentan la longitud de las columnas en las configuraciones de EF Core. Consecuencia asumida: una entidad puede construirse en un estado inválido si se la instancia sin pasar por un caso de uso.

Los valores en uso:

| Constante | Valor | Entidad |
| --- | --- | --- |
| `MAX_LONGITUD_NOMBRE` | 100 | `CatalogoBase` (Area, TipoSolicitud, Prioridad, EstadoSolicitud) |
| `MAX_LONGITUD_CODIGO` | 40 | `EstadoSolicitud` |
| `MAX_LONGITUD_DESCRIPCION` | 500 | `TipoSolicitud` |
| `MAX_LONGITUD_NOMBRE` | 150 | `Usuario` |
| `MAX_LONGITUD_EMAIL` | 150 | `Usuario` |
| `MAX_LONGITUD_PASSWORD_HASH` | 500 | `Usuario` |
| `MAX_LONGITUD_CODIGO` | 20 | `Solicitud` |
| `MAX_LONGITUD_TITULO` | 150 | `Solicitud` |
| `MAX_LONGITUD_DESCRIPCION` | 2000 | `Solicitud` |
| `MAX_LONGITUD_COMENTARIO` | 1000 | `HistorialEstado` |
| `MAX_LONGITUD_TEXTO` | 1000 | `Comentario` |
| `MAX_LONGITUD_DESCRIPCION` | 200 | `Adjunto` |
| `MAX_LONGITUD_URL` | 500 | `Adjunto` |
| `MAX_LONGITUD_ASUNTO` | 200 | `Notificacion` |
| `MAX_LONGITUD_MENSAJE` | 1000 | `Notificacion` |

Cambiarlos es tocar una constante y regenerar la migración. No requiere ADR: basta con confirmarlos.

## 4. Nombres de los proyectos de la solución

**Estado:** abierta, de forma.

La solución contiene `SB.PortalSolicitudes.API` y `SB.PortalSolicitudes.Infraestructure`, mientras la documentación se refiere a las capas como `Api` e `Infrastructure` (con la grafía inglesa). `Infraestructure` además mezcla el español "Infraestructura" con el inglés "Infrastructure".

Pendiente: renombrar los proyectos para que coincidan con la documentación, o actualizar la documentación para que refleje los nombres reales. Los cuatro proyectos tampoco tienen todavía `ProjectReference` entre sí.

## 5. Reglas de autorización por rol más allá de las transiciones

**Estado:** abierta.

[ADR-0005](ADR-0005-transiciones-dirigidas-por-datos.md) resuelve qué rol puede ejecutar cada cambio de estado, con datos. Queda por definir el resto del alcance por rol: qué solicitudes ve un Analista ("asignadas o disponibles para gestión" admite más de una lectura), si un Solicitante puede editar su solicitud después de crearla y hasta qué estado, y quién puede reasignar.
