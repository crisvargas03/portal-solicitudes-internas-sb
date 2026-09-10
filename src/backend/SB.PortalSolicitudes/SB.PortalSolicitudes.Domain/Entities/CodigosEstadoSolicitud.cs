namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Codigos de los estados del flujo base
/// (Registrada, En analisis, En progreso, En espera del solicitante, Resuelta, Cerrada).
/// Existen para que los datos semilla y las reglas de negocio no dependan de literales
/// sueltos ni de identificadores de base de datos.
/// </summary>
public static class CodigosEstadoSolicitud
{
    public const string REGISTRADA = "REGISTRADA";
    public const string EN_ANALISIS = "EN_ANALISIS";
    public const string EN_PROGRESO = "EN_PROGRESO";
    public const string EN_ESPERA_SOLICITANTE = "EN_ESPERA_SOLICITANTE";
    public const string RESUELTA = "RESUELTA";
    public const string CERRADA = "CERRADA";
}
