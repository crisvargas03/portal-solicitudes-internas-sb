namespace SB.PortalSolicitudes.Domain.Enums;

/// <summary>
/// Resultado del intento de emision de una notificacion. Se persiste como entero.
/// </summary>
public enum EstadoNotificacion
{
    /// <summary>Registrada, pendiente de emitirse por su canal.</summary>
    Pendiente = 1,

    /// <summary>Emitida correctamente por su canal.</summary>
    Enviada = 2,

    /// <summary>El canal reporto un error al emitirla.</summary>
    Fallida = 3
}
