namespace SB.PortalSolicitudes.API.Common;

/// <summary>
/// Detalle de error dentro del sobre estandar (<see cref="RespuestaApi{T}"/>/<see cref="RespuestaApi"/>)
/// cuando <c>Exito</c> es <c>false</c>. Reemplaza el uso de <c>ProblemDetails</c> como forma
/// separada de respuesta: exito y fallo comparten ahora el mismo sobre (ver ADR-0010, amendada).
/// </summary>
public sealed class ErrorApiDto
{
    public required string Codigo { get; init; }

    public required string Detalle { get; init; }

    /// <summary>Solo presente en errores de validacion: mensajes agrupados por campo.</summary>
    public IDictionary<string, string[]>? Errores { get; init; }

    /// <summary>Solo presente en errores no controlados (500): identificador para correlacionar con el log.</summary>
    public string? TraceId { get; init; }
}
