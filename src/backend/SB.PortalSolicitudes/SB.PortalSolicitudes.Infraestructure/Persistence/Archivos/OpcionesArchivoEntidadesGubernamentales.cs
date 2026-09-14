namespace SB.PortalSolicitudes.Infraestructure.Persistence.Archivos;

/// <summary>Enlaza la seccion <c>EntidadesGubernamentales</c> de configuracion (ver ADR-0034).</summary>
public class OpcionesArchivoEntidadesGubernamentales
{
    public const string SECCION = "EntidadesGubernamentales";

    /// <summary>
    /// Ruta del archivo de datos. Relativa al directorio raiz de la aplicacion si no es absoluta.
    /// </summary>
    public string RutaArchivo { get; set; } = string.Empty;
}
