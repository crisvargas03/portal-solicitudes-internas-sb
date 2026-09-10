namespace SB.PortalSolicitudes.Application.Abstractions;

/// <summary>
/// Genera el codigo legible y unico de una <see cref="Domain.Entities.Solicitud"/>
/// (formato <c>SOL-{Anio}-{Ultimo:D4}</c>), race-safe ante creaciones concurrentes dentro
/// del mismo año (ver ADR-0011). Debe invocarse dentro de la transaccion que crea la
/// solicitud, para que el incremento del contador y la insercion se confirmen o reviertan
/// juntos.
/// </summary>
public interface IGeneradorCodigoSolicitud
{
    Task<string> GenerarAsync(int anio, CancellationToken cancellationToken = default);
}
