namespace SB.PortalSolicitudes.API.Common;

/// <summary>
/// Sobre estandar de toda respuesta de la API, exitosa o fallida (ver ADR-0010, amendada:
/// antes el fallo usaba <c>ProblemDetails</c> como forma separada; ahora comparte este
/// mismo sobre para que el cliente parsee siempre la misma forma). Los listados paginados
/// colocan su <c>ResultadoPaginado&lt;T&gt;</c> directamente en <see cref="Datos"/>, sin una
/// segunda forma de respuesta para colecciones. El estado HTTP sigue siendo significativo
/// (400/401/403/404/409/500 segun <see cref="Application.Common.Resultados.TipoError"/>),
/// pero deja de ser el unico medio para que el cliente distinga exito de fallo.
/// </summary>
public class RespuestaApi<T>
{
    public bool Exito { get; init; }

    public T? Datos { get; init; }

    public string? Mensaje { get; init; }

    public ErrorApiDto? Error { get; init; }

    public static RespuestaApi<T> Crear(T datos, string? mensaje = null) =>
        new() { Exito = true, Datos = datos, Mensaje = mensaje };

    public static RespuestaApi<T> CrearError(ErrorApiDto error) => new() { Exito = false, Error = error };
}

/// <summary>Variante sin datos, para operaciones que solo confirman exito (por ejemplo, un borrado).</summary>
public class RespuestaApi
{
    public bool Exito { get; init; }

    public string? Mensaje { get; init; }

    public ErrorApiDto? Error { get; init; }

    public static RespuestaApi Crear(string? mensaje = null) => new() { Exito = true, Mensaje = mensaje };

    public static RespuestaApi CrearError(ErrorApiDto error) => new() { Exito = false, Error = error };
}
