using SB.PortalSolicitudes.Application.Common;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Domain.Entities;
using SB.PortalSolicitudes.Domain.Enums;
using SB.PortalSolicitudes.UnitTests.Comun;

namespace SB.PortalSolicitudes.UnitTests.Application.Common;

/// <summary>
/// Regla de visibilidad por rol (ADR-0012, amendada) y su falla cerrada (ADR-0029). Es la
/// base de toda la autorizacion sobre solicitudes: el listado, el dashboard y cada handler
/// que opera por <c>Id</c> dependen de ella.
/// </summary>
public class AlcanceSolicitudesTests
{
    [Fact]
    public void Calcular_Administrador_NoRestringe()
    {
        Resultado<AlcanceSolicitudes> resultado = AlcanceSolicitudesFactory.Calcular(DatosPrueba.Administrador());

        Assert.True(resultado.EsExitoso);
        Assert.Equal(AlcanceSolicitudes.SinRestriccion(), resultado.Valor);
    }

    [Fact]
    public void Calcular_Solicitante_RestringeASusPropiasSolicitudes()
    {
        Resultado<AlcanceSolicitudes> resultado = AlcanceSolicitudesFactory.Calcular(DatosPrueba.SolicitanteUno());

        Assert.True(resultado.EsExitoso);
        Assert.Equal(new AlcanceSolicitudes(DatosPrueba.ID_SOLICITANTE_UNO, null, false), resultado.Valor);
    }

    [Fact]
    public void Calcular_Analista_RestringeAAsignadasASiMismoYSinAsignar()
    {
        Resultado<AlcanceSolicitudes> resultado = AlcanceSolicitudesFactory.Calcular(DatosPrueba.AnalistaUno());

        Assert.True(resultado.EsExitoso);
        Assert.Equal(new AlcanceSolicitudes(null, DatosPrueba.ID_ANALISTA_UNO, true), resultado.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(RolUsuario.Solicitante)]
    [InlineData(RolUsuario.Analista)]
    public void Calcular_SinIdentificador_FallaCerradoEnVezDeAmpliarLaVisibilidad(RolUsuario? rol)
    {
        Resultado<AlcanceSolicitudes> resultado = AlcanceSolicitudesFactory.Calcular(DatosPrueba.UsuarioActual(null, rol));

        Assert.True(resultado.EsFallido);
        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
    }

    [Fact]
    public void Calcular_RolDesconocido_FallaCerrado()
    {
        Resultado<AlcanceSolicitudes> resultado = AlcanceSolicitudesFactory.Calcular(DatosPrueba.ConRolDesconocido());

        Assert.True(resultado.EsFallido);
        Assert.Equal(TipoError.NoAutorizado, resultado.Error.Tipo);
    }

    [Fact]
    public void Incluye_Solicitante_SoloSusPropiasSolicitudes()
    {
        AlcanceSolicitudes alcance = AlcanceSolicitudesFactory.Calcular(DatosPrueba.SolicitanteUno()).Valor;

        Assert.True(alcance.Incluye(SolicitudDe(DatosPrueba.ID_SOLICITANTE_UNO, DatosPrueba.ID_ANALISTA_DOS)));
        Assert.False(alcance.Incluye(SolicitudDe(DatosPrueba.ID_SOLICITANTE_DOS, null)));
    }

    [Fact]
    public void Incluye_Analista_AsignadasASiMismoOSinAsignar()
    {
        AlcanceSolicitudes alcance = AlcanceSolicitudesFactory.Calcular(DatosPrueba.AnalistaUno()).Valor;

        Assert.True(alcance.Incluye(SolicitudDe(DatosPrueba.ID_SOLICITANTE_UNO, DatosPrueba.ID_ANALISTA_UNO)));
        Assert.True(alcance.Incluye(SolicitudDe(DatosPrueba.ID_SOLICITANTE_DOS, null)));
    }

    [Fact]
    public void Incluye_Analista_NoIncluyeLasAsignadasAOtroAnalista()
    {
        AlcanceSolicitudes alcance = AlcanceSolicitudesFactory.Calcular(DatosPrueba.AnalistaUno()).Valor;

        Assert.False(alcance.Incluye(SolicitudDe(DatosPrueba.ID_SOLICITANTE_UNO, DatosPrueba.ID_ANALISTA_DOS)));
    }

    [Fact]
    public void Incluye_Administrador_TodasLasSolicitudes()
    {
        AlcanceSolicitudes alcance = AlcanceSolicitudesFactory.Calcular(DatosPrueba.Administrador()).Valor;

        Assert.True(alcance.Incluye(SolicitudDe(DatosPrueba.ID_SOLICITANTE_DOS, DatosPrueba.ID_ANALISTA_DOS)));
        Assert.True(alcance.Incluye(SolicitudDe(DatosPrueba.ID_SOLICITANTE_UNO, null)));
    }

    private static Solicitud SolicitudDe(int usuarioSolicitanteId, int? usuarioAsignadoId) =>
        DatosPrueba.Solicitud(DatosPrueba.EnProgreso(), usuarioSolicitanteId, usuarioAsignadoId);
}
