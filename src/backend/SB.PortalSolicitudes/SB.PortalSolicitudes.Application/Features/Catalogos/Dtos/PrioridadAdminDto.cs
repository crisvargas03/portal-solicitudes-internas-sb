namespace SB.PortalSolicitudes.Application.Features.Catalogos.Dtos;

/// <summary>Forma de Prioridad para las pantallas de administracion: incluye inactivas.</summary>
public sealed record PrioridadAdminDto(int Id, string Nombre, int Nivel, bool Activo);
