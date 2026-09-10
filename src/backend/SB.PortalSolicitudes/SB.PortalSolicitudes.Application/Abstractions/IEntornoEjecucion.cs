namespace SB.PortalSolicitudes.Application.Abstractions;

/// <summary>
/// Decide si el entorno actual permite operaciones restringidas, como la carga de datos
/// de demostracion (ver ADR-0008: <c>GET /api/seed</c> solo en Development o con
/// <c>Seed:Habilitado</c>). La implementacion vive en Api, la unica capa que conoce
/// <c>IWebHostEnvironment</c>.
/// </summary>
public interface IEntornoEjecucion
{
    bool SeedHabilitado { get; }
}
