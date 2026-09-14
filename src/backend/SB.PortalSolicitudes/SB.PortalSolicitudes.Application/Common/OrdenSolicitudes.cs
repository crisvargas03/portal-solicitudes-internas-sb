namespace SB.PortalSolicitudes.Application.Common;

/// <summary>Whitelist de columnas ordenables de <c>GET /api/solicitudes</c> (ver ADR-0026).</summary>
public enum OrdenSolicitudes
{
    FechaCreacion,
    Codigo,
    Titulo,

    /// <summary>Nivel de prioridad, sin desempate.</summary>
    Prioridad,

    /// <summary>Prioridad primero, luego fecha de compromiso (vencidas/proximas primero, sin fecha al final).</summary>
    Urgencia,
}

public enum DireccionOrden
{
    Asc,
    Desc,
}
