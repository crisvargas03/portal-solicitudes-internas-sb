using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.ActualizarSolicitud;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarAsignacion;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarEstado;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearAdjunto;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearComentario;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearSolicitud;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerDetalleSolicitud;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerSolicitudesPaginado;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Queries.ObtenerTransicionesDisponibles;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/solicitudes")]
[Authorize]
public class SolicitudesController : ControllerBase
{
    private readonly ICommandMediator _commandMediator;
    private readonly IQueryMediator _queryMediator;

    public SolicitudesController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    {
        _commandMediator = commandMediator;
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPaginado(
        [FromQuery] ObtenerSolicitudesPaginadoQuery consulta, CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(consulta, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSolicitudCommand comando, CancellationToken cancellationToken)
    {
        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerDetalle(int id, CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerDetalleSolicitudQuery(id), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarSolicitudRequest cuerpo, CancellationToken cancellationToken)
    {
        ActualizarSolicitudCommand comando = new(
            id, cuerpo.Titulo, cuerpo.Descripcion, cuerpo.TipoSolicitudId, cuerpo.PrioridadId, cuerpo.AreaId,
            cuerpo.FechaCompromiso);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id, [FromBody] CambiarEstadoRequest cuerpo, CancellationToken cancellationToken)
    {
        CambiarEstadoCommand comando = new(id, cuerpo.EstadoDestinoId, cuerpo.Comentario);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpGet("{id:int}/transiciones")]
    public async Task<IActionResult> ObtenerTransicionesDisponibles(int id, CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(
            new ObtenerTransicionesDisponiblesQuery(id), cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}/asignacion")]
    [Authorize(Roles = "Administrador,Analista")]
    public async Task<IActionResult> CambiarAsignacion(
        int id, [FromBody] CambiarAsignacionRequest cuerpo, CancellationToken cancellationToken)
    {
        CambiarAsignacionCommand comando = new(id, cuerpo.UsuarioAsignadoId);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost("{id:int}/comentarios")]
    public async Task<IActionResult> CrearComentario(
        int id, [FromBody] CrearComentarioRequest cuerpo, CancellationToken cancellationToken)
    {
        CrearComentarioCommand comando = new(id, cuerpo.Texto, cuerpo.EsInterno);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost("{id:int}/adjuntos")]
    public async Task<IActionResult> CrearAdjunto(
        int id, [FromBody] CrearAdjuntoRequest cuerpo, CancellationToken cancellationToken)
    {
        CrearAdjuntoCommand comando = new(id, cuerpo.Descripcion, cuerpo.Url);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}

/// <summary>Cuerpo de <c>PATCH /api/solicitudes/{id}</c>: sin <c>Id</c>, que viene de la ruta.</summary>
public sealed record ActualizarSolicitudRequest(
    string? Titulo,
    string? Descripcion,
    int? TipoSolicitudId,
    int? PrioridadId,
    int? AreaId,
    DateTime? FechaCompromiso);

public sealed record CambiarEstadoRequest(int EstadoDestinoId, string? Comentario);

/// <summary><c>UsuarioAsignadoId</c> nulo desasigna.</summary>
public sealed record CambiarAsignacionRequest(int? UsuarioAsignadoId);

public sealed record CrearComentarioRequest(string Texto, bool EsInterno);

public sealed record CrearAdjuntoRequest(string Descripcion, string Url);
