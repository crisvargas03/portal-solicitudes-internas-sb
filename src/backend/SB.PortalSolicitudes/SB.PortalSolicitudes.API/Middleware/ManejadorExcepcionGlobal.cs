using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.Domain.Exceptions;

namespace SB.PortalSolicitudes.API.Middleware;

/// <summary>
/// Ultimo eslabon: cualquier excepcion que llegue aqui no era el fallo esperado que
/// <see cref="Resultado"/> modela (ver ADR-0010). <c>DominioException</c> se traduce a 400;
/// el resto, a 500 generico. El mensaje real y la pila solo van al log — nunca a la
/// respuesta, para no filtrar detalles internos fuera de Development.
/// </summary>
public class ManejadorExcepcionGlobal : IExceptionHandler
{
    private readonly ILogger<ManejadorExcepcionGlobal> _logger;

    public ManejadorExcepcionGlobal(ILogger<ManejadorExcepcionGlobal> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        bool esExcepcionDeDominio = exception is DominioException;
        int estadoHttp = esExcepcionDeDominio ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;

        _logger.LogError(exception, "Excepcion no controlada. TraceId: {TraceId}", traceId);

        ProblemDetails detalle = new()
        {
            Status = estadoHttp,
            Title = esExcepcionDeDominio ? exception.Message : "Ocurrio un error inesperado.",
            Extensions = { ["traceId"] = traceId }
        };

        httpContext.Response.StatusCode = estadoHttp;
        await httpContext.Response.WriteAsJsonAsync(detalle, cancellationToken);

        return true;
    }
}
