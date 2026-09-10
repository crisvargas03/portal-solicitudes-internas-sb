namespace SB.PortalSolicitudes.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta violar una invariante o regla de negocio del dominio.
/// </summary>
public class DominioException : Exception
{
    public DominioException(string mensaje)
        : base(mensaje)
    {
    }

    public DominioException(string mensaje, Exception excepcionInterna)
        : base(mensaje, excepcionInterna)
    {
    }
}
