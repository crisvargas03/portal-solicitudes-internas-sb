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
            .Where(solicitud => filtro.UsuarioAsignadoId == null
                || solicitud.UsuarioAsignadoId == filtro.UsuarioAsignadoId
                || (filtro.IncluirSinAsignar && solicitud.UsuarioAsignadoId == null))
            // Corte aditivo por asignacion (ADR-0026): nunca reemplaza las clausulas de alcance
            // de arriba, solo estrecha lo que ya dejan ver.
            .Where(solicitud => !filtro.SoloSinAsignar || solicitud.UsuarioAsignadoId == null)
            .Where(solicitud => filtro.AsignadasAUsuarioId == null || solicitud.UsuarioAsignadoId == filtro.AsignadasAUsuarioId)
            .Where(solicitud => filtro.FechaCreacionDesde == null || solicitud.FechaCreacion >= filtro.FechaCreacionDesde)
            .Where(solicitud => filtro.FechaCreacionHasta == null || solicitud.FechaCreacion <= filtro.FechaCreacionHasta)
            .Where(solicitud => string.IsNullOrWhiteSpace(filtro.TextoBusqueda)
                || solicitud.Codigo.Contains(filtro.TextoBusqueda)
                || solicitud.Titulo.Contains(filtro.TextoBusqueda))
            // Misma definicion de "vencida" que ContarVencidasAsync y EstaVencida (ADR-0028):
            // un estado final nunca cuenta como vencido, aunque su fecha de compromiso ya paso.
            .Where(solicitud => !filtro.SoloVencidas
                || (solicitud.FechaCompromiso != null
                    && solicitud.FechaCompromiso < filtro.FechaReferencia
                    && !solicitud.Estado!.EsFinal));

        int totalElementos = await consulta.CountAsync(cancellationToken);

        // Se acota antes de tocar la base de datos: el metadato de ResultadoPaginado ya lo
        // hace, pero solo despues de que el Skip/Take de abajo ya ejecuto con el valor crudo.
        int tamanoPagina = Math.Clamp(
            filtro.TamanoPagina, ParametrosPaginacion.PAGINA_MINIMA, ParametrosPaginacion.TAMANO_PAGINA_MAXIMO);
        int pagina = Math.Max(filtro.Pagina, ParametrosPaginacion.PAGINA_MINIMA);

        List<Solicitud> elementos = await AplicarOrden(consulta, filtro.Orden, filtro.Direccion)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<Solicitud>(elementos, totalElementos, pagina, tamanoPagina);
    }

    /// <summary>Whitelist de ordenamiento (ADR-0026): el cliente nunca elige una columna arbitraria.</summary>
    private static IOrderedQueryable<Solicitud> AplicarOrden(
        IQueryable<Solicitud> consulta, OrdenSolicitudes orden, DireccionOrden direccion)
    {
        bool descendente = direccion == DireccionOrden.Desc;

        return orden switch
        {
            OrdenSolicitudes.Codigo => Ordenar(consulta, solicitud => solicitud.Codigo, descendente),
            OrdenSolicitudes.Titulo => Ordenar(consulta, solicitud => solicitud.Titulo, descendente),
            OrdenSolicitudes.Prioridad => Ordenar(consulta, solicitud => solicitud.Prioridad!.Nivel, descendente),
            OrdenSolicitudes.Urgencia => consulta
                .OrderByDescending(solicitud => solicitud.Prioridad!.Nivel)
                .ThenBy(solicitud => solicitud.FechaCompromiso == null)
                .ThenBy(solicitud => solicitud.FechaCompromiso),
            _ => Ordenar(consulta, solicitud => solicitud.FechaCreacion, descendente),
        };
    }

    private static IOrderedQueryable<Solicitud> Ordenar<TClave>(
        IQueryable<Solicitud> consulta, System.Linq.Expressions.Expression<Func<Solicitud, TClave>> clave, bool descendente) =>
        descendente ? consulta.OrderByDescending(clave) : consulta.OrderBy(clave);

    public async Task<IReadOnlyDictionary<string, int>> ContarPorCodigoDeEstadoAsync(
        CriterioDashboard criterio, CancellationToken cancellationToken = default)
    {
        List<(string Codigo, int Cantidad)> conteos = await AplicarCriterio(Conjunto.AsNoTracking(), criterio)
            .GroupBy(solicitud => solicitud.Estado!.Codigo)
            .Select(grupo => new ValueTuple<string, int>(grupo.Key, grupo.Count()))
            .ToListAsync(cancellationToken);

        return conteos.ToDictionary(conteo => conteo.Codigo, conteo => conteo.Cantidad);
    }

    public async Task<IReadOnlyDictionary<int, int>> ContarPorPrioridadAsync(
        CriterioDashboard criterio, CancellationToken cancellationToken = default)
    {
        List<(int PrioridadId, int Cantidad)> conteos = await AplicarCriterio(Conjunto.AsNoTracking(), criterio)
            .GroupBy(solicitud => solicitud.PrioridadId)
            .Select(grupo => new ValueTuple<int, int>(grupo.Key, grupo.Count()))
            .ToListAsync(cancellationToken);

        return conteos.ToDictionary(conteo => conteo.PrioridadId, conteo => conteo.Cantidad);
    }

    public async Task<int> ContarVencidasAsync(
        DateTime fechaReferencia, CriterioDashboard criterio, CancellationToken cancellationToken = default)
    {
        return await AplicarCriterio(Conjunto.AsNoTracking(), criterio)
            .Where(solicitud => solicitud.FechaCompromiso != null && solicitud.FechaCompromiso < fechaReferencia)
            .Where(solicitud => !solicitud.Estado!.EsFinal)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Solicitud>> ObtenerRecientesAsync(
        int cantidad, CriterioDashboard criterio, CancellationToken cancellationToken = default)
    {
        return await AplicarCriterio(Conjunto.AsNoTracking(), criterio)
            .Include(solicitud => solicitud.Estado)
            .Include(solicitud => solicitud.Prioridad)
            .Include(solicitud => solicitud.Area)
            .Include(solicitud => solicitud.TipoSolicitud)
            .Include(solicitud => solicitud.UsuarioSolicitante)
            .Include(solicitud => solicitud.UsuarioAsignado)
            .OrderByDescending(solicitud => solicitud.FechaCreacion)
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarSinAsignarAsync(AlcanceSolicitudes alcance, CancellationToken cancellationToken = default)
    {
        return await AplicarAlcance(Conjunto.AsNoTracking(), alcance)
            .Where(solicitud => solicitud.UsuarioAsignadoId == null)
            .CountAsync(cancellationToken);
    }

    public async Task<int> ContarPorAnioDeCreacionAsync(int anio, CancellationToken cancellationToken = default)
    {
        return await Conjunto.AsNoTracking()
            .Where(solicitud => solicitud.FechaCreacion.Year == anio)
            .CountAsync(cancellationToken);
    }

    private static IQueryable<Solicitud> AplicarAlcance(IQueryable<Solicitud> consulta, AlcanceSolicitudes alcance)
    {
        return consulta
            .Where(solicitud => alcance.UsuarioSolicitanteId == null
                || solicitud.UsuarioSolicitanteId == alcance.UsuarioSolicitanteId)
            .Where(solicitud => alcance.UsuarioAsignadoId == null
                || solicitud.UsuarioAsignadoId == alcance.UsuarioAsignadoId
                || (alcance.IncluirSinAsignar && solicitud.UsuarioAsignadoId == null));
    }

    /// <summary>
    /// Igual que el corte aditivo de <see cref="ObtenerPaginadoAsync"/>: el alcance por rol
    /// nunca se toca, el criterio de asignacion solo estrecha (ADR-0027).
    /// </summary>
    private static IQueryable<Solicitud> AplicarCriterio(IQueryable<Solicitud> consulta, CriterioDashboard criterio)
    {
        return AplicarAlcance(consulta, criterio.Alcance)
            .Where(solicitud => !criterio.SoloSinAsignar || solicitud.UsuarioAsignadoId == null)
            .Where(solicitud => criterio.AsignadasAUsuarioId == null || solicitud.UsuarioAsignadoId == criterio.AsignadasAUsuarioId);
    }
}
