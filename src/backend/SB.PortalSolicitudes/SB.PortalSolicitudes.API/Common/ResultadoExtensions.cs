using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.Application.Common.Resultados;

namespace SB.PortalSolicitudes.API.Common;

/// <summary>
/// Traduce un <see cref="Resultado{T}"/> de un handler de MediatR a la respuesta HTTP
/// estandar: <see cref="RespuestaApi{T}"/> en exito, <c>ProblemDetails</c> (RFC 7807) en
/// fallo. Mantiene a los controladores como una sola linea de retorno.
/// </summary>
public static class ResultadoExtensions
{
    public static IActionResult AResultadoHttp<T>(this Resultado<T> resultado, string? mensajeExito = null)
    {
        if (resultado.EsExitoso)
        {
            return new OkObjectResult(RespuestaApi<T>.Crear(resultado.Valor, mensajeExito));
        }

        return ProblemDetailsDesdeError(resultado.Error);
    }

    public static IActionResult AResultadoHttp(this Resultado resultado, string? mensajeExito = null)
    {
        if (resultado.EsExitoso)
        {
            return new OkObjectResult(RespuestaApi.Crear(mensajeExito));
        }

        return ProblemDetailsDesdeError(resultado.Error);
    }

    private static ObjectResult ProblemDetailsDesdeError(Error error)
    {
        int estadoHttp = error.Tipo switch
        {
            TipoError.Validacion => StatusCodes.Status400BadRequest,
            TipoError.NoAutorizado => StatusCodes.Status401Unauthorized,
            TipoError.Prohibido => StatusCodes.Status403Forbidden,
            TipoError.NoEncontrado => StatusCodes.Status404NotFound,
            TipoError.Conflicto => StatusCodes.Status409Conflict,
            TipoError.Falla => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails detalle = new()
        {
            Title = error.Codigo,
            Detail = error.Descripcion,
            Status = estadoHttp
        };

        return new ObjectResult(detalle) { StatusCode = estadoHttp };
    }
}
