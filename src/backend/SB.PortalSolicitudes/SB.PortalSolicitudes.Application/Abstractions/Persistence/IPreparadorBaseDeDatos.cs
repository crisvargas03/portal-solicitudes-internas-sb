namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>Aplica las migraciones pendientes de EF Core (ver ADR-0008). Solo lo invoca el handler de seed.</summary>
public interface IPreparadorBaseDeDatos
{
    Task AplicarMigracionesPendientesAsync(CancellationToken cancellationToken = default);
}
