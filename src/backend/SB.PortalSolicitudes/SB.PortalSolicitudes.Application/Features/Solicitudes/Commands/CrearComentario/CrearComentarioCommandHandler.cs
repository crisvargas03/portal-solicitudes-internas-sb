using LiteBus.Commands.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearComentario;

public class CrearComentarioCommandHandler : ICommandHandler<CrearComentarioCommand, Resultado<ComentarioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IProveedorFechaHora _proveedorFechaHora;

    public CrearComentarioCommandHandler(
        IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IProveedorFechaHora proveedorFechaHora)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _proveedorFechaHora = proveedorFechaHora;
    }

    public async Task<Resultado<ComentarioDto>> HandleAsync(
        CrearComentarioCommand command, CancellationToken cancellationToken = default)
    {
        if (_usuarioActual.Id is null || _usuarioActual.Rol is null)
        {
            return Resultado.Fallido<ComentarioDto>(Error.NoAutorizado("Auth.NoAutenticado", "No hay una sesion activa."));
        }

        Resultado<AlcanceSolicitudes> alcanceResultado = AlcanceSolicitudesFactory.Calcular(_usuarioActual);
        if (alcanceResultado.EsFallido)
        {
            return Resultado.Fallido<ComentarioDto>(alcanceResultado.Error);
        }

        Solicitud? solicitud = await _unitOfWork.Solicitudes.ObtenerParaCambioDeEstadoAsync(
            command.SolicitudId, cancellationToken);

        bool esSolicitante = _usuarioActual.Rol == RolUsuario.Solicitante;

        if (solicitud is null || !alcanceResultado.Valor.Incluye(solicitud))
        {
            return Resultado.Fallido<ComentarioDto>(Error.NoEncontrado("Solicitud.NoEncontrada", "La solicitud no existe."));
        }

        // Un Solicitante no puede crear comentarios internos: solo el personal que gestiona
        // la solicitud los ve (ver docs/CLAUDE.md).
        bool esInterno = !esSolicitante && command.EsInterno;

        Comentario comentario = new()
        {
            SolicitudId = solicitud.Id,
            UsuarioId = _usuarioActual.Id.Value,
            Texto = command.Texto,
            EsInterno = esInterno,
            Fecha = _proveedorFechaHora.Ahora
        };

        await _unitOfWork.Comentarios.AgregarAsync(comentario, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        Usuario autor = (await _unitOfWork.Usuarios.ObtenerPorIdAsync(_usuarioActual.Id.Value, cancellationToken))!;

        return new ComentarioDto(
            comentario.Id, comentario.Texto, comentario.EsInterno, MapeosSolicitud.AUsuarioResumenDto(autor), comentario.Fecha);
    }
}
