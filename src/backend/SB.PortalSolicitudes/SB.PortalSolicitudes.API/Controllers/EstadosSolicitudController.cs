using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerEstadosSolicitudActivos;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/estados-solicitud")]
[Authorize]
public class EstadosSolicitudController : ControllerBase
{
    private readonly IQueryMediator _queryMediator;

    public EstadosSolicitudController(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivos(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerEstadosSolicitudActivosQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}
