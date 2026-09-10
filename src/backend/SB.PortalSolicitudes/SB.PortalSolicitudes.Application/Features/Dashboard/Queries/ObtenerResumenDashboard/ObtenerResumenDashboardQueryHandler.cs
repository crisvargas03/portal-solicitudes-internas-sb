using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Dashboard.Dtos;
using SB.PortalSolicitudes.Application.Features.Solicitudes;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Dashboard.Queries.ObtenerResumenDashboard;

public class ObtenerResumenDashboardQueryHandler : IQueryHandler<ObtenerResumenDashboardQuery, Resultado<ResumenDashboardDto>>
{
    private const int CANTIDAD_RECIENTES = 5;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;

    public ObtenerResumenDashboardQueryHandler(
        IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IProveedorFechaHora proveedorFechaHora)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
    }

    public async Task<Resultado<ResumenDashboardDto>> HandleAsync(
        ObtenerResumenDashboardQuery query, CancellationToken cancellationToken = default)
    {
        AlcanceSolicitudes alcance = AlcanceSolicitudesFactory.Calcular(_usuarioActual);
        DateTime ahora = _proveedorFechaHora.Ahora;

        IReadOnlyDictionary<string, int> conteoPorEstado =
            await _unitOfWork.Solicitudes.ContarPorCodigoDeEstadoAsync(alcance, cancellationToken);
        IReadOnlyDictionary<int, int> conteoPorPrioridad =
            await _unitOfWork.Solicitudes.ContarPorPrioridadAsync(alcance, cancellationToken);
        int totalVencidas = await _unitOfWork.Solicitudes.ContarVencidasAsync(ahora, alcance, cancellationToken);
        IReadOnlyList<Solicitud> recientes =
            await _unitOfWork.Solicitudes.ObtenerRecientesAsync(CANTIDAD_RECIENTES, alcance, cancellationToken);

        IReadOnlyList<EstadoSolicitud> estados = await _unitOfWork.EstadosSolicitud.ObtenerActivosOrdenadosAsync(cancellationToken);
        IReadOnlyList<Prioridad> prioridades = await _unitOfWork.Prioridades.ObtenerActivasOrdenadasPorNivelAsync(cancellationToken);

        List<ConteoEstadoDto> porEstado = estados
            .Select(estado => new ConteoEstadoDto(
                estado.Codigo, estado.Nombre, conteoPorEstado.GetValueOrDefault(estado.Codigo)))
            .ToList();

        List<ConteoPrioridadDto> porPrioridad = prioridades
            .Select(prioridad => new ConteoPrioridadDto(
                prioridad.Id, prioridad.Nombre, conteoPorPrioridad.GetValueOrDefault(prioridad.Id)))
            .ToList();

        int totalSolicitudes = porEstado.Sum(conteo => conteo.Cantidad);

        List<SolicitudResumenDto> recientesDto = recientes.Select(MapeosSolicitud.ASolicitudResumenDto).ToList();

        return new ResumenDashboardDto(porEstado, porPrioridad, totalVencidas, totalSolicitudes, recientesDto);
    }
}
