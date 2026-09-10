using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerSolicitudesPaginado;

/// <summary>
/// Se une directamente a los parametros de <c>GET /api/solicitudes</c>. No incluye
/// <c>IncluirSinAsignar</c> ni una referencia de fecha: esos los calcula el handler segun
/// el rol (ver <see cref="AlcanceSolicitudesFactory"/>), el cliente no los decide.
/// </summary>
public sealed record ObtenerSolicitudesPaginadoQuery(
    int? EstadoId,
    int? PrioridadId,
    int? AreaId,
    int? TipoSolicitudId,
    int? UsuarioSolicitanteId,
    int? UsuarioAsignadoId,
    DateTime? FechaCreacionDesde,
    DateTime? FechaCreacionHasta,
    string? TextoBusqueda,
    bool SoloVencidas,
    int Pagina = ParametrosPaginacion.PAGINA_MINIMA,
    int TamanoPagina = ParametrosPaginacion.TAMANO_PAGINA_PREDETERMINADO)
    : IQuery<Resultado<ResultadoPaginado<SolicitudResumenDto>>>;
