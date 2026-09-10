namespace SB.PortalSolicitudes.Application.Abstractions.Persistence;

/// <summary>
/// Coordina los repositorios sobre un unico <c>DbContext</c> y expone el punto donde se
/// confirman los cambios. Los handlers de MediatR dependen solo de esta interfaz: no
/// conocen EF Core ni el <c>DbContext</c> concreto (ver docs/architecture.md).
/// La implementacion no expone <see cref="IDisposable"/>: el ciclo de vida del
/// <c>DbContext</c> lo controla el contenedor de dependencias (registro <c>Scoped</c>).
/// </summary>
public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }

    ISolicitudRepository Solicitudes { get; }

    IAreaRepository Areas { get; }

    ITipoSolicitudRepository TiposSolicitud { get; }

    IPrioridadRepository Prioridades { get; }

    IEstadoSolicitudRepository EstadosSolicitud { get; }

    ITransicionPermitidaRepository TransicionesPermitidas { get; }

    IHistorialEstadoRepository HistorialEstados { get; }

    IComentarioRepository Comentarios { get; }

    IAdjuntoRepository Adjuntos { get; }

    INotificacionRepository Notificaciones { get; }

    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Abre una transaccion explicita, para las operaciones que requieren mas de un
    /// <see cref="GuardarCambiosAsync"/> (por ejemplo: cambiar el estado de una solicitud
    /// y registrar el historial en la misma operacion). Lanza <see cref="InvalidOperationException"/>
    /// si ya hay una transaccion abierta.
    /// </summary>
    Task IniciarTransaccionAsync(CancellationToken cancellationToken = default);

    Task ConfirmarTransaccionAsync(CancellationToken cancellationToken = default);

    Task RevertirTransaccionAsync(CancellationToken cancellationToken = default);
}
