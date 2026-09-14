using System.Runtime.ExceptionServices;
using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;

namespace SB.PortalSolicitudes.Application.Common.Comportamientos;

/// <summary>
/// Log estructurado de inicio/fin/falla de cada comando. Abierto sobre un unico parametro
/// generico en las tres interfaces (el unico admitido por LiteBus para handlers abiertos).
/// <see cref="HandleErrorAsync"/> relanza la excepcion despues de loguearla: LiteBus
/// suprime la excepcion original en cuanto hay un <c>ICommandErrorHandler</c> registrado
/// para el comando, y sin relanzar, el middleware de excepciones de la Api (ADR-0010)
/// nunca la veria — ver ADR-0014 para la evidencia de este comportamiento.
/// </summary>
public class RegistroComandos<TCommand> :
    ICommandPreHandler<TCommand>,
    ICommandPostHandler<TCommand>,
    ICommandErrorHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ILogger<RegistroComandos<TCommand>> _logger;

    public RegistroComandos(ILogger<RegistroComandos<TCommand>> logger)
    {
        _logger = logger;
    }

    public Task PreHandleAsync(TCommand message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando comando {Comando}", typeof(TCommand).Name);

        return Task.CompletedTask;
    }

    public Task PostHandleAsync(TCommand message, object? result, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Comando {Comando} completado", typeof(TCommand).Name);

        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(
        TCommand message, object? messageResult, Exception exception, CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "Comando {Comando} fallo", typeof(TCommand).Name);

        ExceptionDispatchInfo.Capture(exception).Throw();

        return Task.CompletedTask;
    }
}
