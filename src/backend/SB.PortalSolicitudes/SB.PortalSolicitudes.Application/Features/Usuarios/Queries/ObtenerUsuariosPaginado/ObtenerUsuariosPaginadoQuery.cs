using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Enums;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;

/// <summary>
/// Con <c>Rol</c> definido, el handler ignora la paginacion y devuelve la lista completa
/// (uso principal: poblar el selector de responsable en el frontend). Sin <c>Rol</c>,
/// pagina de verdad (uso principal: administracion de usuarios).
/// </summary>
public sealed record ObtenerUsuariosPaginadoQuery(
    RolUsuario? Rol,
    bool SoloActivos = true,
    int Pagina = ParametrosPaginacion.PAGINA_MINIMA,
    int TamanoPagina = ParametrosPaginacion.TAMANO_PAGINA_PREDETERMINADO)
    : IQuery<Resultado<ResultadoPaginado<UsuarioResumenDto>>>;
