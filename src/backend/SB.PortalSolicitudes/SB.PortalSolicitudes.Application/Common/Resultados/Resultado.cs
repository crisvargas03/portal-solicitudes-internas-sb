namespace SB.PortalSolicitudes.Application.Common.Resultados;

/// <summary>
/// Resultado de una operacion sin valor de retorno. Los handlers de MediatR lo usan para
/// modelar los fallos esperados (validacion, no encontrado, conflicto, autorizacion) como
/// datos en lugar de excepciones; las excepciones siguen siendo para lo verdaderamente
/// excepcional.
/// </summary>
public class Resultado
{
    public bool EsExitoso { get; }

    public bool EsFallido => !EsExitoso;

    public Error Error { get; }

    protected Resultado(bool esExitoso, Error error)
    {
        if (esExitoso && error != Error.Ninguno)
        {
            throw new InvalidOperationException("Un resultado exitoso no puede llevar un error.");
        }

        if (!esExitoso && error == Error.Ninguno)
        {
            throw new InvalidOperationException("Un resultado fallido debe llevar un error.");
        }

        EsExitoso = esExitoso;
        Error = error;
    }

    public static Resultado Exitoso() => new(true, Error.Ninguno);

    public static Resultado Fallido(Error error) => new(false, error);

    public static Resultado<T> Exitoso<T>(T valor) => new(valor, true, Error.Ninguno);

    public static Resultado<T> Fallido<T>(Error error) => new(default!, false, error);
}

/// <summary>
/// Resultado de una operacion que produce un valor. <see cref="Valor"/> solo es valido
/// cuando <see cref="Resultado.EsExitoso"/> es <c>true</c>.
/// </summary>
public class Resultado<T> : Resultado
{
    private readonly T _valor;

    public T Valor => EsExitoso
        ? _valor
        : throw new InvalidOperationException(
            $"No se puede leer el valor de un resultado fallido (error: {Error.Codigo}).");

    protected internal Resultado(T valor, bool esExitoso, Error error) : base(esExitoso, error)
    {
        _valor = valor;
    }

    public static implicit operator Resultado<T>(T valor) => Exitoso(valor);
}
