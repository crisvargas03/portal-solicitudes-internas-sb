namespace SB.PortalSolicitudes.API.Common;

/// <summary>
/// Sobre estandar de toda respuesta exitosa de la API. Los listados paginados colocan su
/// <c>ResultadoPaginado&lt;T&gt;</c> directamente en <see cref="Datos"/>, sin una segunda
/// forma de respuesta para colecciones.
/// Las respuestas de error no usan este sobre: usan <c>ProblemDetails</c> (RFC 7807),
/// ver <see cref="ResultadoExtensions"/>.
/// </summary>
public class RespuestaApi<T>
{
    public bool Exito { get; init; } = true;

    public T? Datos { get; init; }

    public string? Mensaje { get; init; }

    public static RespuestaApi<T> Crear(T datos, string? mensaje = null) =>
        new() { Exito = true, Datos = datos, Mensaje = mensaje };
}

/// <summary>Variante sin datos, para operaciones que solo confirman exito (por ejemplo, un borrado).</summary>
public class RespuestaApi
{
    public bool Exito { get; init; } = true;

    public string? Mensaje { get; init; }

    public static RespuestaApi Crear(string? mensaje = null) => new() { Exito = true, Mensaje = mensaje };
}
