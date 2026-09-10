namespace SB.PortalSolicitudes.Domain.Enums;

/// <summary>
/// Medio por el que se emitio (o simulo) una notificacion. Se persiste como entero.
/// El envio real esta fuera de alcance: ver ADR-0003.
/// </summary>
public enum CanalNotificacion
{
    /// <summary>Salida por consola / log.</summary>
    Consola = 1,

    /// <summary>Correo simulado, sin envio real.</summary>
    CorreoSimulado = 2,

    /// <summary>Solo se persiste el registro en base de datos.</summary>
    BaseDeDatos = 3
}
