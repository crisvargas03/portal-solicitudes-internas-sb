using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerSolicitudesPaginado;

public class ObtenerSolicitudesPaginadoQueryHandler
    : IQueryHandler<ObtenerSolicitudesPaginadoQuery, Resultado<ResultadoPaginado<SolicitudResumenDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;

    public ObtenerSolicitudesPaginadoQueryHandler(
        IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IProveedorFechaHora proveedorFechaHora)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
    }

    public async Task<Resultado<ResultadoPaginado<SolicitudResumenDto>>> HandleAsync(
        ObtenerSolicitudesPaginadoQuery query, CancellationToken cancellationToken = default)
    {
        Resultado<AlcanceSolicitudes> alcanceResultado = AlcanceSolicitudesFactory.Calcular(_usuarioActual);
        if (alcanceResultado.EsFallido)
        {
            return Resultado.Fallido<ResultadoPaginado<SolicitudResumenDto>>(alcanceResultado.Error);
        }

        AlcanceSolicitudes alcance = alcanceResultado.Valor;
        DateTime ahora = _proveedorFechaHora.Ahora;

        FiltroSolicitudes filtro = new()
        {
            EstadoId = query.EstadoId,
            PrioridadId = query.PrioridadId,
            AreaId = query.AreaId,
            TipoSolicitudId = query.TipoSolicitudId,
            UsuarioSolicitanteId = alcance.UsuarioSolicitanteId ?? query.UsuarioSolicitanteId,
            UsuarioAsignadoId = alcance.UsuarioAsignadoId ?? query.UsuarioAsignadoId,
            IncluirSinAsignar = alcance.IncluirSinAsignar,
            SoloSinAsignar = query.Asignacion == FiltroAsignacion.Disponibles,
            AsignadasAUsuarioId = query.Asignacion == FiltroAsignacion.Asignadas ? _usuarioActual.Id : null,
            FechaCreacionDesde = query.FechaCreacionDesde,
            FechaCreacionHasta = query.FechaCreacionHasta,
            TextoBusqueda = query.TextoBusqueda,
            SoloVencidas = query.SoloVencidas,
            FechaReferencia = ahora,
            Orden = query.Orden,
            Direccion = query.Direccion,
            Pagina = query.Pagina,
            TamanoPagina = query.TamanoPagina
        };

        ResultadoPaginado<Solicitud> pagina = await _unitOfWork.Solicitudes.ObtenerPaginadoAsync(filtro, cancellationToken);

        List<SolicitudResumenDto> elementos = pagina.Elementos
            .Select(solicitud => MapeosSolicitud.ASolicitudResumenDto(solicitud, ahora))
            .ToList();

        return new ResultadoPaginado<SolicitudResumenDto>(elementos, pagina.TotalElementos, pagina.Pagina, pagina.TamanoPagina);
    }
}
