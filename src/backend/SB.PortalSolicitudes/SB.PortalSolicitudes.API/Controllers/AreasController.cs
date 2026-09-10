using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerAreasActivas;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/areas")]
[Authorize]
public class AreasController : ControllerBase
{
    private readonly IQueryMediator _queryMediator;

    public AreasController(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(new ObtenerAreasActivasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}
