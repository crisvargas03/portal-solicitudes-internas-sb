# ADR-0022: Gestión de usuarios — soft delete y guarda "Administrador no modifica Administrador"

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

`GET/POST/PATCH /api/usuarios[/{id}]` (listado paginado y filtrable por `Rol`, creación, actualización con `Activo` como bandera de baja) ya existían y cubrían "CRUD" salvo dos cosas: (1) no había ninguna regla que impidiera a un Administrador modificar o desactivar a otro Administrador, y (2) quedaba sin definir si "eliminar" un usuario debía ser una baja lógica (reutilizando el `Activo` del `PATCH` existente) o un borrado físico con un endpoint `DELETE` nuevo.

`Usuario` tiene referencias entrantes desde `Solicitud` (`UsuarioSolicitanteId`, `UsuarioAsignadoId`) y `Comentario` (`UsuarioId`). Un borrado físico de un usuario con solicitudes o comentarios históricos rompería esas referencias o exigiría una política de cascada/reasignación que el dominio no define hoy.

## Decisión

**Baja de usuario:** solo baja lógica. Se reutiliza el `PATCH /api/usuarios/{id}` existente con `Activo: false` — no se agrega un endpoint `DELETE`. Un usuario desactivado conserva su historial de solicitudes y comentarios intacto.

**Administrador no modifica Administrador:** se agrega una guarda dentro de `ActualizarUsuarioCommandHandler` (mismo patrón que ya usan `CambiarAsignacionCommandHandler`/`ActualizarSolicitudCommandHandler` para sus propias reglas de autorización de grano fino: una comprobación en el handler, no un atributo o política nueva a nivel de framework):

```csharp
bool esOtroAdministrador = usuario.Rol == RolUsuario.Administrador && usuario.Id != _usuarioActual.Id;
if (esOtroAdministrador) return Resultado.Fallido(...Prohibido...);
```

Un Administrador puede seguir editando su propia cuenta (`usuario.Id == _usuarioActual.Id` es la excepción explícita); la guarda solo bloquea actuar sobre *otro* Administrador. Como no existe un endpoint de borrado físico, esta guarda cubre el único punto de mutación de usuario que existe.

## Consecuencias

- **A favor:** ningún dato histórico de `Solicitud`/`Comentario` puede quedar huérfano por una baja de usuario; la regla de "no tocar a otro Admin" vive en el mismo lugar donde el proyecto ya resuelve autorización de grano fino específico de un caso, sin introducir una capa de políticas nueva solo para esto; un Administrador conserva la capacidad de autoadministrarse.
- **En contra / trade-offs:** un usuario desactivado sigue existiendo indefinidamente en la base de datos — no hay un mecanismo de purga; si en el futuro se necesita un borrado físico real (cumplimiento, GDPR-like), habrá que decidir entonces qué pasa con sus solicitudes y comentarios, algo que esta decisión no resuelve, solo pospone.
- **Alternativas descartadas:** un atributo/política de autorización declarativa a nivel de ASP.NET Core para "no Admin sobre Admin" — descartada porque la regla depende del usuario *destino* de la petición (el recurso, no el rol de quien la hace), algo que las políticas basadas en rol de `[Authorize]` no expresan sin lógica adicional idéntica a la que ya vive en el handler; agregar un `DELETE` físico con reasignación automática de sus solicitudes a un "usuario sistema" — descartada por ser una regla de negocio nueva y no solicitada, más compleja que la baja lógica ya disponible.
