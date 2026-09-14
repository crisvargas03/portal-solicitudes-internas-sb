using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Auth.Dtos;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Auth.Commands.RenovarSesion;

public class RenovarSesionCommandHandler : ICommandHandler<RenovarSesionCommand, Resultado<SesionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorTokens _proveedorTokens;
    private readonly ILogger<RenovarSesionCommandHandler> _logger;

    public RenovarSesionCommandHandler(
        IUnitOfWork unitOfWork,
        IUsuarioActual usuarioActual,
        IProveedorTokens proveedorTokens,
        ILogger<RenovarSesionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorTokens = proveedorTokens;
        _logger = logger;
    }

    public async Task<Resultado<SesionDto>> HandleAsync(
        RenovarSesionCommand command, CancellationToken cancellationToken = default)
    {
        if (!_usuarioActual.EstaAutenticado || _usuarioActual.Id is null)
        {
            return Resultado.Fallido<SesionDto>(
                Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        Usuario? usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(_usuarioActual.Id.Value, cancellationToken);

        // Revalida existencia y Activo: sin esto, un usuario desactivado despues de emitido
        // su token podria seguir renovando esa sesion indefinidamente (ver ADR-0019).
        if (usuario is null || !usuario.Activo)
        {
            _logger.LogWarning("Renovacion de sesion rechazada para UsuarioId={UsuarioId}", _usuarioActual.Id);

            return Resultado.Fallido<SesionDto>(
                Error.NoAutorizado("Auth.SesionInvalida", "La sesion ya no es valida."));
        }

        (string token, DateTime expiraEn) = _proveedorTokens.GenerarToken(usuario);

        _logger.LogInformation("Sesion renovada para UsuarioId={UsuarioId}", usuario.Id);

        UsuarioResumenDto usuarioDto = new(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString(), usuario.Activo);

        return new SesionDto(token, expiraEn, usuarioDto);
    }
}
