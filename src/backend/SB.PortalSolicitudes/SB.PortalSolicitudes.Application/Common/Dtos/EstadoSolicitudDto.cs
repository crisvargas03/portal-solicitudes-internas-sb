namespace SB.PortalSolicitudes.Application.Common.Dtos;

public sealed record EstadoSolicitudDto(int Id, string Codigo, string Nombre, int Orden, bool EsFinal);
