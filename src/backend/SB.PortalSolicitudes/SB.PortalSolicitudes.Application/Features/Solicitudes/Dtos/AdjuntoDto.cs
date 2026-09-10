using SB.PortalSolicitudes.Application.Common.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

public sealed record AdjuntoDto(int Id, string Descripcion, string Url, UsuarioResumenDto Usuario, DateTime Fecha);
