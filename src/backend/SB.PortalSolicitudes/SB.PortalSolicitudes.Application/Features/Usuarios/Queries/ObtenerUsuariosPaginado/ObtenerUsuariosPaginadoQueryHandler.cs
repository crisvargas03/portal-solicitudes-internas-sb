using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Dtos;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;

public class ObtenerUsuariosPaginadoQueryHandler
    : IQueryHandler<ObtenerUsuariosPaginadoQuery, Resultado<ResultadoPaginado<UsuarioResumenDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerUsuariosPaginadoQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<ResultadoPaginado<UsuarioResumenDto>>> HandleAsync(
        ObtenerUsuariosPaginadoQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Rol is not null)
        {
            IReadOnlyList<Usuario> usuarios =
                await _unitOfWork.Usuarios.ObtenerPorRolAsync(query.Rol.Value, query.SoloActivos, cancellationToken);

            List<UsuarioResumenDto> todos = usuarios.Select(AUsuarioResumenDto).ToList();

            return new ResultadoPaginado<UsuarioResumenDto>(todos, todos.Count, ParametrosPaginacion.PAGINA_MINIMA, Math.Max(todos.Count, 1));
        }

        ParametrosPaginacion paginacion = new() { Pagina = query.Pagina, TamanoPagina = query.TamanoPagina };

        ResultadoPaginado<Usuario> pagina =
            await _unitOfWork.Usuarios.ObtenerPaginadoAsync(paginacion, query.SoloActivos, cancellationToken);

        List<UsuarioResumenDto> elementos = pagina.Elementos.Select(AUsuarioResumenDto).ToList();

        return new ResultadoPaginado<UsuarioResumenDto>(elementos, pagina.TotalElementos, pagina.Pagina, pagina.TamanoPagina);
    }

    private static UsuarioResumenDto AUsuarioResumenDto(Usuario usuario) =>
        new(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString(), usuario.Activo);
}
