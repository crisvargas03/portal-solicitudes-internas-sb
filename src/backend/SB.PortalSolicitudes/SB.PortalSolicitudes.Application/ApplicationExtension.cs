using System.Reflection;
using FluentValidation;
using LiteBus.Commands;
using LiteBus.Extensions.Microsoft.DependencyInjection;
using LiteBus.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace SB.PortalSolicitudes.Application;

/// <summary>
/// Registro de los servicios de la capa de aplicacion: LiteBus (comandos y consultas,
/// ver ADR-0014) y los validadores de FluentValidation de <c>Application/Features</c>.
/// Los handlers de logging/validacion abiertos (<c>Application/Common/Comportamientos</c>)
/// los descubre <c>RegisterFromAssembly</c> automaticamente, sin registro aparte.
/// </summary>
public static class ApplicationExtension
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        Assembly ensamblado = typeof(ApplicationExtension).Assembly;

        servicios.AddLiteBus(bus =>
        {
            bus.AddCommandModule(modulo => modulo.RegisterFromAssembly(ensamblado));
            bus.AddQueryModule(modulo => modulo.RegisterFromAssembly(ensamblado));
        });

        servicios.AddValidatorsFromAssembly(ensamblado);

        return servicios;
    }
}
