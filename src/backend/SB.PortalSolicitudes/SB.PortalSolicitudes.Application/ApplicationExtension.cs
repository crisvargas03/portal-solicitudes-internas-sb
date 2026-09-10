using Microsoft.Extensions.DependencyInjection;

namespace SB.PortalSolicitudes.Application;

/// <summary>
/// Registro de los servicios de la capa de aplicacion. Todavia no registra nada: es el
/// punto donde se agregara <c>AddMediatR</c> junto con los handlers de comandos y
/// consultas (<c>Application/Features</c>).
/// </summary>
public static class ApplicationExtension
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        return servicios;
    }
}
