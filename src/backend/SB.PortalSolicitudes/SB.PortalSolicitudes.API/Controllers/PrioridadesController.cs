using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Catalogos.Queries.ObtenerPrioridadesActivas;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/prioridades")]
[Authorize]
public class PrioridadesController : ControllerBase
{
    private readonly IQueryMediator _queryMediator;

    public PrioridadesController(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerActivas(CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerPrioridadesActivasQuery(), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}
