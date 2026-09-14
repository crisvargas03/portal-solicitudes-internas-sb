namespace SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

/// <summary>Forma de Area para las pantallas de administracion: incluye inactivas.</summary>
public sealed record AreaAdminDto(int Id, string Nombre, bool Activo);
