namespace SB.PortalSolicitudes.Application.Abstractions;

/// <summary>
/// Fuente unica de la hora actual para los handlers, para que <c>FechaCreacion</c> y las
/// consultas por <c>FechaReferencia</c> (vencidas, dashboard) sean inyectables y testeables
/// en lugar de leer <c>DateTime.UtcNow</c> directamente.
/// </summary>
public interface IProveedorFechaHora
{
    DateTime Ahora { get; }
}
