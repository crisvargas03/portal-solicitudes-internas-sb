using System.Runtime.ExceptionServices;
using LiteBus.Queries.Abstractions;
using Microsoft.Extensions.Logging;

namespace SB.PortalSolicitudes.Application.Common.Comportamientos;

/// <summary>Version para consultas de <see cref="RegistroComandos{TCommand}"/> — ver esa clase.</summary>
public class RegistroConsultas<TQuery> :
    IQueryPreHandler<TQuery>,
    IQueryPostHandler<TQuery>,
    IQueryErrorHandler<TQuery>
    where TQuery : IQuery
{
    private readonly ILogger<RegistroConsultas<TQuery>> _logger;

    public RegistroConsultas(ILogger<RegistroConsultas<TQuery>> logger)
    {
        _logger = logger;
    }

    public Task PreHandleAsync(TQuery message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando consulta {Consulta}", typeof(TQuery).Name);

        return Task.CompletedTask;
    }

    public Task PostHandleAsync(TQuery message, object? result, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consulta {Consulta} completada", typeof(TQuery).Name);

        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(
        TQuery message, object? messageResult, Exception exception, CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "Consulta {Consulta} fallo", typeof(TQuery).Name);

        ExceptionDispatchInfo.Capture(exception).Throw();

        return Task.CompletedTask;
    }
}
