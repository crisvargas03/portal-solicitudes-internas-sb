namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;

/// <summary>Forma de EntidadGubernamental expuesta por la Api: incluye inactivas cuando el endpoint lo permite.</summary>
public sealed record EntidadGubernamentalAdminDto(
    int Id, string Nombre, string Categoria, string PoderDelEstado, string Sector, bool Activo);
