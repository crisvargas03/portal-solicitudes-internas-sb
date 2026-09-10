# Decisiones abiertas

Registro de lo que el requerimiento no especifica y todavía no se ha decidido. Cuando una entrada se resuelve, se convierte en un ADR en `docs/ADR/` y se retira de esta lista.

Referenciado por `docs/CLAUDE.md` ("si estás a punto de decidir algo que no es explícito, no lo decidas en silencio: márcalo y regístralo aquí") y por `docs/conventions.md`.

---

## 1. Longitudes máximas de los campos de texto

**Estado:** valores provisionales, pendientes de confirmación.

El requerimiento exige "validación de campos obligatorios y longitud máxima de texto" pero no da cifras.

Las entidades de `Domain` **no validan**: solo declaran las constantes. La comprobación de campos obligatorios, longitudes y formatos vive en la capa `Application` (validadores de DTO, ver ADR-0013), y las mismas constantes alimentan la longitud de las columnas en las configuraciones de EF Core. Consecuencia asumida: una entidad puede construirse en un estado inválido si se la instancia sin pasar por un caso de uso.

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

## 2. Nombres de los proyectos de la solución

**Estado:** abierta, de forma.

La solución contiene `SB.PortalSolicitudes.API` y `SB.PortalSolicitudes.Infraestructure`, mientras la documentación se refiere a las capas como `Api` e `Infrastructure` (con la grafía inglesa). `Infraestructure` además mezcla el español "Infraestructura" con el inglés "Infrastructure".

Pendiente: renombrar los proyectos para que coincidan con la documentación, o actualizar la documentación para que refleje los nombres reales. Los cuatro proyectos ya tienen `ProjectReference` entre sí (ver ADR-0009 y ADR-0014).
