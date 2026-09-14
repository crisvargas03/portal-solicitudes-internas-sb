using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes;

/// <summary>
/// Valida que las referencias a catalogos (Area, TipoSolicitud, Prioridad) que llegan en un
/// comando existan y esten activas, antes de que la FK de la base de datos las rechace con un
/// 500 en vez de un 400 (ver checklist, punto 6).
/// </summary>
public static class ValidadorReferenciasSolicitud
{
    public static async Task<Error?> ValidarAsync(
        IUnitOfWork unitOfWork, int areaId, int tipoSolicitudId, int prioridadId, CancellationToken cancellationToken)
    {
        Area? area = await unitOfWork.Areas.ObtenerPorIdAsync(areaId, cancellationToken);
        if (area is null || !area.Activo)
        {
            return Error.Validacion("Solicitud.AreaInvalida", "El area indicada no existe o esta inactiva.");
        }

        TipoSolicitud? tipo = await unitOfWork.TiposSolicitud.ObtenerPorIdAsync(tipoSolicitudId, cancellationToken);
        if (tipo is null || !tipo.Activo)
        {
            return Error.Validacion(
                "Solicitud.TipoSolicitudInvalido", "El tipo de solicitud indicado no existe o esta inactivo.");
        }

        Prioridad? prioridad = await unitOfWork.Prioridades.ObtenerPorIdAsync(prioridadId, cancellationToken);
        if (prioridad is null || !prioridad.Activo)
        {
            return Error.Validacion("Solicitud.PrioridadInvalida", "La prioridad indicada no existe o esta inactiva.");
        }

        return null;
    }
}
