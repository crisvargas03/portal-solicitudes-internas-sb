using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerTiposSolicitudActivos;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/tipos-solicitud")]
[Authorize]
public class TiposSolicitudController : ControllerBase
{
    private readonly IQueryMediator _queryMediator;

    public TiposSolicitudController(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivos(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerTiposSolicitudActivosQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}
