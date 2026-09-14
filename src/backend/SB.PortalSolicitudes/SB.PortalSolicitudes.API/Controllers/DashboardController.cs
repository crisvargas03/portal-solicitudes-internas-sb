using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Dashboard.Queries.ObtenerResumenDashboard;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IQueryMediator _queryMediator;

    public DashboardController(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen(
        [FromQuery] ObtenerResumenDashboardQuery consulta, CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(consulta, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}
