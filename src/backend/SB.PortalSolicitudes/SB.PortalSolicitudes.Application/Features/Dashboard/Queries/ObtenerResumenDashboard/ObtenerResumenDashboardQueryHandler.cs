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
        Resultado<AlcanceSolicitudes> alcanceResultado = AlcanceSolicitudesFactory.Calcular(_usuarioActual);
        if (alcanceResultado.EsFallido)
        {
            return Resultado.Fallido<ResumenDashboardDto>(alcanceResultado.Error);
        }

        AlcanceSolicitudes alcance = alcanceResultado.Valor;
        DateTime ahora = _proveedorFechaHora.Ahora;

        CriterioDashboard criterio = query.Asignacion switch
        {
            FiltroAsignacion.Asignadas => new CriterioDashboard(alcance, _usuarioActual.Id, false),
            FiltroAsignacion.Disponibles => new CriterioDashboard(alcance, null, true),
            _ => CriterioDashboard.DeAlcance(alcance),
        };

        IReadOnlyDictionary<string, int> conteoPorEstado =
            await _unitOfWork.Solicitudes.ContarPorCodigoDeEstadoAsync(criterio, cancellationToken);
        IReadOnlyDictionary<int, int> conteoPorPrioridad =
            await _unitOfWork.Solicitudes.ContarPorPrioridadAsync(criterio, cancellationToken);
        int totalVencidas = await _unitOfWork.Solicitudes.ContarVencidasAsync(ahora, criterio, cancellationToken);
        IReadOnlyList<Solicitud> recientes =
            await _unitOfWork.Solicitudes.ObtenerRecientesAsync(CANTIDAD_RECIENTES, criterio, cancellationToken);
        int totalSinAsignar = await _unitOfWork.Solicitudes.ContarSinAsignarAsync(alcance, cancellationToken);

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

        List<SolicitudResumenDto> recientesDto = recientes
            .Select(solicitud => MapeosSolicitud.ASolicitudResumenDto(solicitud, ahora))
            .ToList();

        return new ResumenDashboardDto(porEstado, porPrioridad, totalVencidas, totalSolicitudes, recientesDto, totalSinAsignar);
    }
}
