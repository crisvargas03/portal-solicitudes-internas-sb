using SB.PortalSolicitudes.Domain.Common;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Rol autorizado a ejecutar una <see cref="TransicionPermitida"/>. Se modela como tabla
/// hija en lugar de una enumeracion de banderas, para no depender de valores compuestos.
/// </summary>
public class TransicionPermitidaRol : EntidadBase
{
    public int TransicionPermitidaId { get; set; }

    public TransicionPermitida? TransicionPermitida { get; set; }

    public RolUsuario Rol { get; set; }
}
