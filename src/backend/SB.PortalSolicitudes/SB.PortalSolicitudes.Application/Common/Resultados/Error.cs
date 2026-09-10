namespace SB.PortalSolicitudes.Application.Common.Resultados;

/// <summary>
/// Error de negocio devuelto por un handler. <see cref="Codigo"/> es una clave estable
/// con puntos (por ejemplo <c>Solicitud.NoEncontrada</c>) pensada para que el cliente
/// distinga casos sin tener que interpretar <see cref="Descripcion"/>.
/// </summary>
public sealed record Error(string Codigo, string Descripcion, TipoError Tipo)
{
    public static readonly Error Ninguno = new(string.Empty, string.Empty, TipoError.Falla);

    public static Error Validacion(string codigo, string descripcion) =>
        new(codigo, descripcion, TipoError.Validacion);

    public static Error NoEncontrado(string codigo, string descripcion) =>
        new(codigo, descripcion, TipoError.NoEncontrado);

    public static Error Conflicto(string codigo, string descripcion) =>
        new(codigo, descripcion, TipoError.Conflicto);

    public static Error NoAutorizado(string codigo, string descripcion) =>
        new(codigo, descripcion, TipoError.NoAutorizado);

    public static Error Prohibido(string codigo, string descripcion) =>
        new(codigo, descripcion, TipoError.Prohibido);

    public static Error Falla(string codigo, string descripcion) =>
        new(codigo, descripcion, TipoError.Falla);
}
