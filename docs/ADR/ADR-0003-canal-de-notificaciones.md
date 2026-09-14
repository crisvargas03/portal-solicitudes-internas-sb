# ADR-0003: Canal de notificaciones

**Estado:** Aceptada
**Fecha:** 2026-09-10

## Contexto

El requerimiento pide notificar (o simular la notificación) en creación, asignación, cambio de estado y cierre de una solicitud, y deja el mecanismo abierto: registro en base de datos, consola, correo simulado o evento en cola. `Notificacion`, `CanalNotificacion` y `EstadoNotificacion` ya existían en `Domain` sin comportamiento de envío (ver `docs/open-decisions.md`, entrada retirada por este ADR). Quedaba pendiente decidir la interfaz de despacho y qué canal(es) implementar primero.

## Decisión

`INotificationService` (`Application/Abstractions/Notificaciones`) es el punto único que los handlers invocan al ocurrir un evento de negocio; despacha a todos los `INotificationChannel` registrados. Se implementa un solo canal, `CanalNotificacionBaseDeDatos` (Infraestructure), que persiste el registro en `Notificacion` (`Estado = Enviada`) y lo refleja en el log estructurado de Serilog. **No hay envío real de correo ni de cola**: el alcance sigue siendo simulado, tal como preveía la entrada de decisiones abiertas.

Los handlers llaman a `NotificarAsync` después de confirmar su propia transacción (`CrearSolicitudCommandHandler`, `CambiarEstadoCommandHandler`, `CambiarAsignacionCommandHandler`), nunca dentro de ella: un fallo al notificar no debe revertir la operación de negocio que la origina.

## Consecuencias

- **A favor:** los handlers no conocen el canal concreto — agregar correo simulado o cola más adelante es implementar `INotificationChannel` y registrarlo, sin tocar `Application`; el registro en `Notificacion` deja auditoría consultable vía `GET /api/notificaciones`.
- **En contra / trade-offs:** con un solo canal, la abstracción `IEnumerable<INotificationChannel>` no se prueba todavía con más de una implementación real; si notificar falla después de confirmar la transacción de negocio, esa operación ya quedó guardada pese al fallo de notificación (aceptado: notificar es una consecuencia del evento, no una condición para que ocurra).
- **Alternativas descartadas:** notificar dentro de la misma transacción que la operación de negocio, descartada porque un fallo de notificación no debe poder revertir un cambio de estado o una creación ya válidos.
