using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerNotificacionesPaginado;
using SB.PortalSolicitudes.Application.Features.Notificaciones.Queries.ObtenerResumenNotificaciones;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly IQueryMediator _queryMediator;

    public NotificacionesController(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPaginado(
        [FromQuery] ObtenerNotificacionesPaginadoQuery consulta, CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(consulta, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerResumenNotificacionesQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}
