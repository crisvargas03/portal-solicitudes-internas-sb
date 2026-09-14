using SB.PortalSolicitudes.Application.Abstractions;

namespace SB.PortalSolicitudes.Infraestructure;

public class ProveedorFechaHora : IProveedorFechaHora
{
    public DateTime Ahora => DateTime.UtcNow;
}
