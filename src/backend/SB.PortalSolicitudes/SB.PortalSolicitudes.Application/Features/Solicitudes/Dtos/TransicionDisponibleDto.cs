namespace SB.PortalSolicitudes.Application.Features.Solicitudes.Dtos;

/// <summary>
/// Un estado alcanzable desde el estado actual de la solicitud, para el rol del usuario
/// autenticado (ver ADR-0005 y ADR-0012). El frontend la usa para mostrar solo botones de
/// transicion validas y forzar el cuadro de comentario cuando corresponde, sin
/// reimplementar la maquina de estados en TypeScript.
/// </summary>
public sealed record TransicionDisponibleDto(int EstadoDestinoId, string Codigo, string Nombre, bool RequiereComentario);
