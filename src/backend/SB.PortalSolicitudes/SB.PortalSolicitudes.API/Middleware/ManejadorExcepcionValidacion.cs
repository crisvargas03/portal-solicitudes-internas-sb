using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SB.PortalSolicitudes.API.Middleware;

/// <summary>
/// Traduce la <see cref="ValidationException"/> de FluentValidation (lanzada por el
/// pre-handler de validacion, ver ADR-0013) a <c>ValidationProblemDetails</c> con el
/// diccionario de errores por campo que el frontend necesita.
/// </summary>
public class ManejadorExcepcionValidacion : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException excepcionValidacion)
        {
            return false;
        }

        Dictionary<string, string[]> errores = excepcionValidacion.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(error => error.ErrorMessage).ToArray());

        ValidationProblemDetails detalle = new(errores)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Uno o mas campos no son validos."
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(detalle, cancellationToken);

        return true;
    }
}
