using Microsoft.EntityFrameworkCore.Storage;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

namespace SB.PortalSolicitudes.Infraestructure.Persistence;

/// <summary>
/// Implementacion de <see cref="IUnitOfWork"/> sobre <see cref="PortalSolicitudesDbContext"/>.
/// Los repositorios se crean de forma perezosa y comparten el mismo contexto, por lo que
/// comparten tambien un unico <c>ChangeTracker</c> y un unico <c>SaveChanges</c>.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PortalSolicitudesDbContext _contexto;
    private IDbContextTransaction? _transaccionActual;

    private IUsuarioRepository? _usuarios;
    private ISolicitudRepository? _solicitudes;
    private IAreaRepository? _areas;
    private ITipoSolicitudRepository? _tiposSolicitud;
    private IPrioridadRepository? _prioridades;
    private IEstadoSolicitudRepository? _estadosSolicitud;
    private ITransicionPermitidaRepository? _transicionesPermitidas;
    private IHistorialEstadoRepository? _historialEstados;
    private IComentarioRepository? _comentarios;
    private IAdjuntoRepository? _adjuntos;
    private INotificacionRepository? _notificaciones;

    public UnitOfWork(PortalSolicitudesDbContext contexto)
    {
        _contexto = contexto;
    }

    public IUsuarioRepository Usuarios => _usuarios ??= new UsuarioRepository(_contexto);

    public ISolicitudRepository Solicitudes => _solicitudes ??= new SolicitudRepository(_contexto);

    public IAreaRepository Areas => _areas ??= new AreaRepository(_contexto);

    public ITipoSolicitudRepository TiposSolicitud => _tiposSolicitud ??= new TipoSolicitudRepository(_contexto);

    public IPrioridadRepository Prioridades => _prioridades ??= new PrioridadRepository(_contexto);

    public IEstadoSolicitudRepository EstadosSolicitud => _estadosSolicitud ??= new EstadoSolicitudRepository(_contexto);

    public ITransicionPermitidaRepository TransicionesPermitidas =>
        _transicionesPermitidas ??= new TransicionPermitidaRepository(_contexto);

    public IHistorialEstadoRepository HistorialEstados =>
        _historialEstados ??= new HistorialEstadoRepository(_contexto);

    public IComentarioRepository Comentarios => _comentarios ??= new ComentarioRepository(_contexto);

    public IAdjuntoRepository Adjuntos => _adjuntos ??= new AdjuntoRepository(_contexto);

    public INotificacionRepository Notificaciones => _notificaciones ??= new NotificacionRepository(_contexto);

    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task IniciarTransaccionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaccionActual is not null)
        {
            throw new InvalidOperationException("Ya hay una transaccion abierta en esta unidad de trabajo.");
        }

        _transaccionActual = await _contexto.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task ConfirmarTransaccionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaccionActual is null)
        {
            throw new InvalidOperationException("No hay una transaccion abierta para confirmar.");
        }

        try
        {
            await _contexto.SaveChangesAsync(cancellationToken);
            await _transaccionActual.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaccionActual.DisposeAsync();
            _transaccionActual = null;
        }
    }

    public async Task RevertirTransaccionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaccionActual is null)
        {
            throw new InvalidOperationException("No hay una transaccion abierta para revertir.");
        }

        try
        {
            await _transaccionActual.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaccionActual.DisposeAsync();
            _transaccionActual = null;
        }
    }
}
