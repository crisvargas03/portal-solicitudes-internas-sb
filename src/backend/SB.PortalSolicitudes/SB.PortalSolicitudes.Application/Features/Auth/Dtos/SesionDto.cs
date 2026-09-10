using SB.PortalSolicitudes.Application.Common.Dtos;

namespace SB.PortalSolicitudes.Application.Features.Auth.Dtos;

public sealed record SesionDto(string Token, DateTime ExpiraEn, UsuarioResumenDto Usuario);
