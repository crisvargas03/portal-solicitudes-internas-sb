using FluentValidation.Results;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.ActualizarSolicitud;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CambiarEstado;
using SB.PortalSolicitudes.Application.Features.Solicitudes.Commands.CrearSolicitud;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Solicitudes;

/// <summary>
/// Campos obligatorios y longitud maxima de los textos principales (requerimiento de la
/// seccion 5), validados en el backend aunque el frontend ya los valide (ADR-0013). Los
/// limites se leen de las constantes de la entidad, igual que el validador.
/// </summary>
public class ValidadoresSolicitudTests
{
    private const int ID_INVALIDO = 0;

    private readonly CrearSolicitudCommandValidator _validadorCrear = new();
    private readonly ActualizarSolicitudCommandValidator _validadorActualizar = new();
    private readonly CambiarEstadoCommandValidator _validadorCambiarEstado = new();

    [Fact]
    public void CrearSolicitud_ComandoValido_NoTieneErrores()
    {
        ValidationResult resultado = _validadorCrear.Validate(ComandoCrear());

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CrearSolicitud_TituloVacio_EsInvalido(string titulo)
    {
        ValidationResult resultado = _validadorCrear.Validate(ComandoCrear() with { Titulo = titulo });

        AfirmarErrorEn(resultado, nameof(CrearSolicitudCommand.Titulo));
    }

    [Fact]
    public void CrearSolicitud_TituloEnElLimite_EsValido()
    {
        ValidationResult resultado = _validadorCrear.Validate(
            ComandoCrear() with { Titulo = new string('a', Solicitud.MAX_LONGITUD_TITULO) });

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void CrearSolicitud_TituloExcedeLongitudMaxima_EsInvalido()
    {
        ValidationResult resultado = _validadorCrear.Validate(
            ComandoCrear() with { Titulo = new string('a', Solicitud.MAX_LONGITUD_TITULO + 1) });

        AfirmarErrorEn(resultado, nameof(CrearSolicitudCommand.Titulo));
    }

    [Fact]
    public void CrearSolicitud_DescripcionExcedeLongitudMaxima_EsInvalido()
    {
        ValidationResult resultado = _validadorCrear.Validate(
            ComandoCrear() with { Descripcion = new string('a', Solicitud.MAX_LONGITUD_DESCRIPCION + 1) });

        AfirmarErrorEn(resultado, nameof(CrearSolicitudCommand.Descripcion));
    }

    [Fact]
    public void CrearSolicitud_CatalogosSinSeleccionar_ReportaCadaCampo()
    {
        ValidationResult resultado = _validadorCrear.Validate(
            ComandoCrear() with { TipoSolicitudId = ID_INVALIDO, PrioridadId = ID_INVALIDO, AreaId = ID_INVALIDO });

        AfirmarErrorEn(resultado, nameof(CrearSolicitudCommand.TipoSolicitudId));
        AfirmarErrorEn(resultado, nameof(CrearSolicitudCommand.PrioridadId));
        AfirmarErrorEn(resultado, nameof(CrearSolicitudCommand.AreaId));
    }

    [Fact]
    public void ActualizarSolicitud_CamposNulos_SignificanSinCambiosYSonValidos()
    {
        ValidationResult resultado = _validadorActualizar.Validate(
            new ActualizarSolicitudCommand(DatosPrueba.ID_SOLICITUD, null, null, null, null, null, null));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void ActualizarSolicitud_TituloEnviadoVacio_EsInvalido()
    {
        ValidationResult resultado = _validadorActualizar.Validate(
            new ActualizarSolicitudCommand(DatosPrueba.ID_SOLICITUD, "", null, null, null, null, null));

        AfirmarErrorEn(resultado, nameof(ActualizarSolicitudCommand.Titulo));
    }

    [Fact]
    public void CambiarEstado_ComentarioExcedeLongitudMaxima_EsInvalido()
    {
        ValidationResult resultado = _validadorCambiarEstado.Validate(new CambiarEstadoCommand(
            DatosPrueba.ID_SOLICITUD, DatosPrueba.ID_ESTADO_RESUELTA, new string('a', HistorialEstado.MAX_LONGITUD_COMENTARIO + 1)));

        AfirmarErrorEn(resultado, nameof(CambiarEstadoCommand.Comentario));
    }

    private static CrearSolicitudCommand ComandoCrear() => new(
        "Falla de impresora", "La impresora del piso 3 no responde.",
        DatosPrueba.ID_CATALOGO, DatosPrueba.ID_CATALOGO, DatosPrueba.ID_CATALOGO, FechaCompromiso: null);

    private static void AfirmarErrorEn(ValidationResult resultado, string propiedad)
    {
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, error => error.PropertyName == propiedad);
    }
}
