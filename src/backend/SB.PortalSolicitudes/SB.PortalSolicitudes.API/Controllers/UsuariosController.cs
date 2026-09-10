using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.Application.Features.Usuarios.Commands.ActualizarUsuario;
using SB.PortalSolicitudes.Application.Features.Usuarios.Commands.CrearUsuario;
using SB.PortalSolicitudes.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Administrador,Analista")]
public class UsuariosController : ControllerBase
{
    private readonly ICommandMediator _commandMediator;
    private readonly IQueryMediator _queryMediator;

    public UsuariosController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    {
        _commandMediator = commandMediator;
        _queryMediator = queryMediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPaginado(
        [FromQuery] ObtenerUsuariosPaginadoQuery consulta, CancellationToken cancellationToken)
    {
        var resultado = await _queryMediator.QueryAsync(consulta, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioCommand comando, CancellationToken cancellationToken)
    {
        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarUsuarioRequest cuerpo, CancellationToken cancellationToken)
    {
        ActualizarUsuarioCommand comando = new(id, cuerpo.Nombre, cuerpo.Rol, cuerpo.Activo);

        var resultado = await _commandMediator.SendAsync(comando, cancellationToken: cancellationToken);

        return resultado.AResultadoHttp();
    }
}

public sealed record ActualizarUsuarioRequest(string? Nombre, RolUsuario? Rol, bool? Activo);
