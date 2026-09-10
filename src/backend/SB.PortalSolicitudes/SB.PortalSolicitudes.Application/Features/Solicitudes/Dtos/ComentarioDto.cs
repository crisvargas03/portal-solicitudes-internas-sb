using SB.PortalSolicitudes.Application.Common.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

public sealed record ComentarioDto(int Id, string Texto, bool EsInterno, UsuarioResumenDto Usuario, DateTime Fecha);
