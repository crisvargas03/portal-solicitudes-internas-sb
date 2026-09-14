namespace SB.PortalSolicitudes.Application.Common.Dtos;

/// <summary>Nunca lleva <c>PasswordHash</c>: es la unica forma en que un Usuario cruza la Api.</summary>
public sealed record UsuarioResumenDto(int Id, string Nombre, string Email, string Rol, bool Activo);
