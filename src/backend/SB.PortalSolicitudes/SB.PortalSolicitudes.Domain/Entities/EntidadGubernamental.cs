namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Entidad gubernamental de Republica Dominicana (catalogo administrable). A diferencia
/// del resto de catalogos, no se persiste en la base de datos relacional sino en un
/// archivo de texto (ver ADR-0034) — por eso no hereda de <c>EntidadBase</c>/<c>CatalogoBase</c>,
/// cuya identidad la asigna el motor de base de datos.
/// </summary>
public class EntidadGubernamental
{
    public const int MAX_LONGITUD_NOMBRE = 200;
    public const int MAX_LONGITUD_CATEGORIA = 100;
    public const int MAX_LONGITUD_PODER_DEL_ESTADO = 100;
    public const int MAX_LONGITUD_SECTOR = 100;

    /// <summary>
    /// Separador de campo del archivo de texto plano que respalda esta entidad (ver ADR-0034).
    /// Los validadores de Application lo prohiben dentro de los campos de texto para que el
    /// archivo nunca necesite un mecanismo de escape.
    /// </summary>
    public const char SEPARADOR_CAMPO_ARCHIVO = '|';

    public int Id { get; set; }

    public required string Nombre { get; set; }

    public required string Categoria { get; set; }

    public required string PoderDelEstado { get; set; }

    public required string Sector { get; set; }

    public bool Activo { get; set; } = true;
}
