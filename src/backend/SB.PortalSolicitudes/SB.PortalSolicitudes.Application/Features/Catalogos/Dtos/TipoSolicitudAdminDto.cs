namespace SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

/// <summary>Forma de TipoSolicitud para las pantallas de administracion: incluye inactivos.</summary>
public sealed record TipoSolicitudAdminDto(int Id, string Nombre, string? Descripcion, bool Activo);
