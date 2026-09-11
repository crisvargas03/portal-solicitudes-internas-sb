using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using SB.PortalSolicitudes.API.Common;

namespace SB.PortalSolicitudes.API.Middleware;

/// <summary>
/// Traduce la <see cref="ValidationException"/> de FluentValidation (lanzada por el
/// pre-handler de validacion, ver ADR-0013) al mismo sobre <see cref="RespuestaApi"/> que
/// usa toda la Api (ver ADR-0010, amendada), con el diccionario de errores por campo que
/// el frontend necesita en <see cref="ErrorApiDto.Errores"/>.
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

        RespuestaApi cuerpo = RespuestaApi.CrearError(new ErrorApiDto
        {
            Codigo = "Validacion",
            Detalle = "Uno o mas campos no son validos.",
            Errores = errores
        });

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(cuerpo, cancellationToken);

        return true;
    }
}
