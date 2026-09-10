namespace SB.PortalSolicitudes.Application.Common.Dtos;

/// <summary>Forma comun de respuesta para los catalogos simples (Area, TipoSolicitud).</summary>
public sealed record CatalogoDto(int Id, string Nombre);
