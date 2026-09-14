using FluentValidation;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales;

/// <summary>
/// Regla de validacion compartida por los comandos de Crear/Actualizar: ningun campo de
/// texto puede contener el separador de campo del archivo (ver ADR-0034), o corromperia
/// el formato leer-todo/reescribir-todo del repositorio.
/// </summary>
public static class EntidadGubernamentalValidacionExtensions
{
    public static IRuleBuilderOptions<T, string?> DebeSerCampoDeArchivoValido<T>(
        this IRuleBuilderOptions<T, string?> reglas)
    {
        return reglas.Must(valor => valor is null || !valor.Contains(EntidadGubernamental.SEPARADOR_CAMPO_ARCHIVO))
            .WithMessage(
                $"El valor no puede contener el caracter '{EntidadGubernamental.SEPARADOR_CAMPO_ARCHIVO}'.");
    }
}
