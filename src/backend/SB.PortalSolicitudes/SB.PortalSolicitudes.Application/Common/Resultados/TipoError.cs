namespace SB.PortalSolicitudes.Application.Common.Resultados;

/// <summary>
/// Categoria de un <see cref="Error"/>. No es un codigo HTTP: <c>Application</c> no conoce
/// HTTP. La capa <c>Api</c> traduce cada categoria a un estado concreto
/// (ver <c>ResultadoExtensions.AResultadoHttp</c>).
/// </summary>
public enum TipoError
{
    Validacion = 1,
    NoEncontrado = 2,
    Conflicto = 3,
    NoAutorizado = 4,
    Prohibido = 5,
    Falla = 6
}
