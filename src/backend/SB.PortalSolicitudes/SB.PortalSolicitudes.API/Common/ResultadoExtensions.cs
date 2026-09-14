using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.Application.Common.Resultados;

namespace SB.PortalSolicitudes.API.Common;

/// <summary>
/// Traduce un <see cref="Resultado{T}"/> de un handler a la respuesta HTTP estandar: el
/// mismo sobre <see cref="RespuestaApi{T}"/> en exito y en fallo (ver ADR-0010, amendada).
/// Mantiene a los controladores como una sola linea de retorno.
/// </summary>
public static class ResultadoExtensions
{
    public static IActionResult AResultadoHttp<T>(this Resultado<T> resultado, string? mensajeExito = null)
    {
        if (resultado.EsExitoso)
        {
            return new OkObjectResult(RespuestaApi<T>.Crear(resultado.Valor, mensajeExito));
        }

        return RespuestaDeError<T>(resultado.Error);
    }

    public static IActionResult AResultadoHttp(this Resultado resultado, string? mensajeExito = null)
    {
        if (resultado.EsExitoso)
        {
            return new OkObjectResult(RespuestaApi.Crear(mensajeExito));
        }

        return RespuestaDeError(resultado.Error);
    }

    public static ObjectResult RespuestaDeError<T>(Error error)
    {
        RespuestaApi<T> cuerpo = RespuestaApi<T>.CrearError(
            new ErrorApiDto { Codigo = error.Codigo, Detalle = error.Descripcion });

        return new ObjectResult(cuerpo) { StatusCode = EstadoHttpDesdeTipoError(error.Tipo) };
    }

    public static ObjectResult RespuestaDeError(Error error)
    {
        RespuestaApi cuerpo = RespuestaApi.CrearError(
            new ErrorApiDto { Codigo = error.Codigo, Detalle = error.Descripcion });

        return new ObjectResult(cuerpo) { StatusCode = EstadoHttpDesdeTipoError(error.Tipo) };
    }

    public static int EstadoHttpDesdeTipoError(TipoError tipo) => tipo switch
    {
        TipoError.Validacion => StatusCodes.Status400BadRequest,
        TipoError.NoAutorizado => StatusCodes.Status401Unauthorized,
        TipoError.Prohibido => StatusCodes.Status403Forbidden,
        TipoError.NoEncontrado => StatusCodes.Status404NotFound,
        TipoError.Conflicto => StatusCodes.Status409Conflict,
        TipoError.Falla => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}
