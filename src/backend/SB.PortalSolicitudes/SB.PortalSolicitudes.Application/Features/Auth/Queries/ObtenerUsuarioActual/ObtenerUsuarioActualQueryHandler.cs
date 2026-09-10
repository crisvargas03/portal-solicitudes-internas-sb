using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Auth.Queries.ObtenerUsuarioActual;

public class ObtenerUsuarioActualQueryHandler
    : IQueryHandler<ObtenerUsuarioActualQuery, Resultado<UsuarioResumenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public ObtenerUsuarioActualQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<Resultado<UsuarioResumenDto>> HandleAsync(
        ObtenerUsuarioActualQuery query, CancellationToken cancellationToken = default)
    {
        if (!_usuarioActual.EstaAutenticado || _usuarioActual.Id is null)
        {
            return Resultado.Fallido<UsuarioResumenDto>(
                Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        Usuario? usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(_usuarioActual.Id.Value, cancellationToken);

        if (usuario is null)
        {
            return Resultado.Fallido<UsuarioResumenDto>(
                Error.NoEncontrado("Usuario.NoEncontrado", "El usuario autenticado ya no existe."));
        }

        return new UsuarioResumenDto(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString());
    }
}
