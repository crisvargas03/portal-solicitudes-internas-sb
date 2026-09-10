using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

public class SolicitudRepository : RepositorioBase<Solicitud>, ISolicitudRepository
{
    public SolicitudRepository(PortalSolicitudesDbContext contexto) : base(contexto)
    {
    }

    public async Task<Solicitud?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Conjunto
            .Include(solicitud => solicitud.Estado)
            .Include(solicitud => solicitud.Prioridad)
            .Include(solicitud => solicitud.Area)
            .Include(solicitud => solicitud.TipoSolicitud)
            .Include(solicitud => solicitud.UsuarioSolicitante)
            .Include(solicitud => solicitud.UsuarioAsignado)
            .Include(solicitud => solicitud.Historial).ThenInclude(historial => historial.EstadoAnterior)
            .Include(solicitud => solicitud.Historial).ThenInclude(historial => historial.EstadoNuevo)
            .Include(solicitud => solicitud.Historial).ThenInclude(historial => historial.Usuario)
            .Include(solicitud => solicitud.Comentarios).ThenInclude(comentario => comentario.Usuario)
            .Include(solicitud => solicitud.Adjuntos).ThenInclude(adjunto => adjunto.Usuario)
            .AsSplitQuery()
            .FirstOrDefaultAsync(solicitud => solicitud.Id == id, cancellationToken);
    }

    public async Task<Solicitud?> ObtenerParaCambioDeEstadoAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Conjunto
            .Include(solicitud => solicitud.Estado)
            .FirstOrDefaultAsync(solicitud => solicitud.Id == id, cancellationToken);
    }

    public async Task<Solicitud?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .FirstOrDefaultAsync(solicitud => solicitud.Codigo == codigo, cancellationToken);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .AnyAsync(solicitud => solicitud.Codigo == codigo, cancellationToken);
    }

    public async Task<ResultadoPaginado<Solicitud>> ObtenerPaginadoAsync(
        FiltroSolicitudes filtro, CancellationToken cancellationToken = default)
    {
        IQueryable<Solicitud> consulta = Conjunto.AsNoTracking()
            .Include(solicitud => solicitud.Estado)
            .Include(solicitud => solicitud.Prioridad)
            .Include(solicitud => solicitud.Area)
            .Include(solicitud => solicitud.TipoSolicitud)
            .Include(solicitud => solicitud.UsuarioSolicitante)
            .Include(solicitud => solicitud.UsuarioAsignado)
            .Where(solicitud => filtro.EstadoId == null || solicitud.EstadoId == filtro.EstadoId)
            .Where(solicitud => filtro.PrioridadId == null || solicitud.PrioridadId == filtro.PrioridadId)
            .Where(solicitud => filtro.AreaId == null || solicitud.AreaId == filtro.AreaId)
            .Where(solicitud => filtro.TipoSolicitudId == null || solicitud.TipoSolicitudId == filtro.TipoSolicitudId)
            .Where(solicitud => filtro.UsuarioSolicitanteId == null || solicitud.UsuarioSolicitanteId == filtro.UsuarioSolicitanteId)
            .Where(solicitud => filtro.UsuarioAsignadoId == null || solicitud.UsuarioAsignadoId == filtro.UsuarioAsignadoId)
            .Where(solicitud => filtro.FechaCreacionDesde == null || solicitud.FechaCreacion >= filtro.FechaCreacionDesde)
            .Where(solicitud => filtro.FechaCreacionHasta == null || solicitud.FechaCreacion <= filtro.FechaCreacionHasta)
            .Where(solicitud => string.IsNullOrWhiteSpace(filtro.TextoBusqueda)
                || solicitud.Codigo.Contains(filtro.TextoBusqueda)
                || solicitud.Titulo.Contains(filtro.TextoBusqueda))
            .Where(solicitud => !filtro.SoloVencidas
                || (solicitud.FechaCompromiso != null && solicitud.FechaCompromiso < filtro.FechaReferencia));

        int totalElementos = await consulta.CountAsync(cancellationToken);

        List<Solicitud> elementos = await consulta
            .OrderByDescending(solicitud => solicitud.FechaCreacion)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<Solicitud>(elementos, totalElementos, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<IReadOnlyDictionary<string, int>> ContarPorCodigoDeEstadoAsync(
        CancellationToken cancellationToken = default)
    {
        List<(string Codigo, int Cantidad)> conteos = await Conjunto.AsNoTracking()
            .GroupBy(solicitud => solicitud.Estado!.Codigo)
            .Select(grupo => new ValueTuple<string, int>(grupo.Key, grupo.Count()))
            .ToListAsync(cancellationToken);

        return conteos.ToDictionary(conteo => conteo.Codigo, conteo => conteo.Cantidad);
    }

    public async Task<IReadOnlyDictionary<int, int>> ContarPorPrioridadAsync(CancellationToken cancellationToken = default)
    {
        List<(int PrioridadId, int Cantidad)> conteos = await Conjunto.AsNoTracking()
            .GroupBy(solicitud => solicitud.PrioridadId)
            .Select(grupo => new ValueTuple<int, int>(grupo.Key, grupo.Count()))
            .ToListAsync(cancellationToken);

        return conteos.ToDictionary(conteo => conteo.PrioridadId, conteo => conteo.Cantidad);
    }

    public async Task<int> ContarVencidasAsync(DateTime fechaReferencia, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(solicitud => solicitud.FechaCompromiso != null && solicitud.FechaCompromiso < fechaReferencia)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Solicitud>> ObtenerRecientesAsync(
        int cantidad, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Include(solicitud => solicitud.Estado)
            .Include(solicitud => solicitud.UsuarioAsignado)
            .OrderByDescending(solicitud => solicitud.FechaCreacion)
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarPorAnioDeCreacionAsync(int anio, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(solicitud => solicitud.FechaCreacion.Year == anio)
            .CountAsync(cancellationToken);
    }
}
