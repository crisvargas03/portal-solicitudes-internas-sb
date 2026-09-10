using SB.PortalSolicitudes.Application.Abstractions;

namespace SB.PortalSolicitudes.API.Autenticacion;

public class EntornoEjecucion : IEntornoEjecucion
{
    public EntornoEjecucion(IWebHostEnvironment entorno, IConfiguration configuracion)
    {
        SeedHabilitado = entorno.IsDevelopment() || configuracion.GetValue<bool>("Seed:Habilitado");
    }

    public bool SeedHabilitado { get; }
}
