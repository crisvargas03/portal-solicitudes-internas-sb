using FluentValidation;
using FluentValidation.Results;
using LiteBus.Queries.Abstractions;

namespace SB.PortalSolicitudes.Application.Common.Comportamientos;

/// <summary>Version para consultas de <see cref="ValidadorComandos{TCommand}"/> — ver esa clase.</summary>
public class ValidadorConsultas<TQuery> : IQueryPreHandler<TQuery>
    where TQuery : IQuery
{
    private readonly IEnumerable<IValidator<TQuery>> _validadores;

    public ValidadorConsultas(IEnumerable<IValidator<TQuery>> validadores)
    {
        _validadores = validadores;
    }

    public async Task PreHandleAsync(TQuery message, CancellationToken cancellationToken = default)
    {
        if (!_validadores.Any())
        {
            return;
        }

        ValidationContext<TQuery> contexto = new(message);

        ValidationResult[] resultados = await Task.WhenAll(
            _validadores.Select(validador => validador.ValidateAsync(contexto, cancellationToken)));

        List<ValidationFailure> fallos = resultados
            .SelectMany(resultado => resultado.Errors)
            .ToList();

        if (fallos.Count > 0)
        {
            throw new ValidationException(fallos);
        }
    }
}
