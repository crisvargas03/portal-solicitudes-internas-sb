using FluentValidation;
using FluentValidation.Results;
using LiteBus.Commands.Abstractions;

namespace SB.PortalSolicitudes.Application.Common.Comportamientos;

/// <summary>
/// Ejecuta los validadores de FluentValidation registrados para <typeparamref name="TCommand"/>
/// antes del handler. Abierto sobre un unico parametro generico, el unico admitido por
/// LiteBus para handlers de este tipo. Si falla, lanza <see cref="ValidationException"/>,
/// capturada por el middleware de excepciones de la Api (ver ADR-0013).
/// </summary>
public class ValidadorComandos<TCommand> : ICommandPreHandler<TCommand>
    where TCommand : ICommand
{
    private readonly IEnumerable<IValidator<TCommand>> _validadores;

    public ValidadorComandos(IEnumerable<IValidator<TCommand>> validadores)
    {
        _validadores = validadores;
    }

    public async Task PreHandleAsync(TCommand message, CancellationToken cancellationToken = default)
    {
        if (!_validadores.Any())
        {
            return;
        }

        ValidationContext<TCommand> contexto = new(message);

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
