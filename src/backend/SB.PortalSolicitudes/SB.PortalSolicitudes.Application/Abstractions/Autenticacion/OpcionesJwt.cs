namespace SB.PortalSolicitudes.Application.Abstractions.Autenticacion;

/// <summary>
/// Enlaza la seccion <c>Jwt</c> de configuracion. Vive en Application porque tanto
/// Infraestructure (emite el token) como Api (valida el token entrante) la necesitan.
/// </summary>
public class OpcionesJwt
{
    public const string SECCION = "Jwt";

    public string Emisor { get; set; } = string.Empty;

    public string Audiencia { get; set; } = string.Empty;

    public string ClaveSecreta { get; set; } = string.Empty;

    public int MinutosExpiracion { get; set; } = 60;
}
