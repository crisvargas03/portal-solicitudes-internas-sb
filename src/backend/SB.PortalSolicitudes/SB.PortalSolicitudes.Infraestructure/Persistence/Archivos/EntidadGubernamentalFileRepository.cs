using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Domain.Entities;

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Archivos;

/// <summary>
/// Implementacion de <see cref="IEntidadGubernamentalRepository"/> respaldada por un
/// archivo de texto plano delimitado (un registro por linea, campos separados por
/// <see cref="SEPARADOR_CAMPO"/>), en vez de la base de datos relacional que usa el resto
/// del proyecto (ver ADR-0034). Los validadores de Crear/Actualizar (Application) prohiben
/// el separador dentro de los campos de texto, asi que no hace falta un mecanismo de escape.
///
/// Estrategia de lectura/escritura: leer-todo/reescribir-todo. Cada escritura vuelca la
/// lista completa a un archivo temporal y lo reemplaza de forma atomica
/// (<see cref="File.Move(string, string, bool)"/> con <c>overwrite: true</c>), para que un
/// corte a mitad de escritura nunca deje el archivo real a medio escribir. Un
/// <see cref="SemaphoreSlim"/> por instancia serializa lecturas y escrituras entre si;
/// la instancia se registra como Singleton (ver <c>InfraestructureExtension</c>) para que
/// esa serializacion cubra a todo el proceso y no solo a una peticion.
/// </summary>
public class EntidadGubernamentalFileRepository : IEntidadGubernamentalRepository
{
    private const char SEPARADOR_CAMPO = EntidadGubernamental.SEPARADOR_CAMPO_ARCHIVO;
    private const int CANTIDAD_CAMPOS = 6;

    private readonly string _rutaArchivo;
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    public EntidadGubernamentalFileRepository(
        IOptions<OpcionesArchivoEntidadesGubernamentales> opciones, IHostEnvironment entorno)
    {
        string rutaConfigurada = opciones.Value.RutaArchivo;

        if (string.IsNullOrWhiteSpace(rutaConfigurada))
        {
            throw new InvalidOperationException(
                $"No se configuro '{OpcionesArchivoEntidadesGubernamentales.SECCION}:RutaArchivo'. " +
                "Defina la ruta del archivo de datos en la configuracion.");
        }

        _rutaArchivo = Path.IsPathRooted(rutaConfigurada)
            ? rutaConfigurada
            : Path.Combine(entorno.ContentRootPath, rutaConfigurada);
    }

    public async Task<IReadOnlyList<EntidadGubernamental>> ObtenerActivasAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EntidadGubernamental> todas = await LeerTodasAsync(cancellationToken);

        return todas.Where(entidad => entidad.Activo).ToList();
    }

    public Task<IReadOnlyList<EntidadGubernamental>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        return LeerTodasAsync(cancellationToken);
    }

    public async Task<EntidadGubernamental?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EntidadGubernamental> todas = await LeerTodasAsync(cancellationToken);

        return todas.FirstOrDefault(entidad => entidad.Id == id);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre, int? idExcluido = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EntidadGubernamental> todas = await LeerTodasAsync(cancellationToken);

        return todas.Any(entidad =>
            string.Equals(entidad.Nombre, nombre, StringComparison.OrdinalIgnoreCase)
            && (idExcluido == null || entidad.Id != idExcluido));
    }

    public async Task<EntidadGubernamental> CrearAsync(
        EntidadGubernamental entidad, CancellationToken cancellationToken = default)
    {
        await _semaforo.WaitAsync(cancellationToken);

        try
        {
            List<EntidadGubernamental> todas = (await LeerSinBloqueoAsync(cancellationToken)).ToList();

            entidad.Id = todas.Count == 0 ? 1 : todas.Max(existente => existente.Id) + 1;
            todas.Add(entidad);

            await EscribirTodasAsync(todas, cancellationToken);

            return entidad;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    public async Task ActualizarAsync(EntidadGubernamental entidad, CancellationToken cancellationToken = default)
    {
        await _semaforo.WaitAsync(cancellationToken);

        try
        {
            List<EntidadGubernamental> todas = (await LeerSinBloqueoAsync(cancellationToken)).ToList();

            int indice = todas.FindIndex(existente => existente.Id == entidad.Id);

            if (indice == -1)
            {
                throw new InvalidOperationException(
                    $"No se encontro la entidad gubernamental con Id {entidad.Id} para actualizar.");
            }

            todas[indice] = entidad;

            await EscribirTodasAsync(todas, cancellationToken);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private async Task<IReadOnlyList<EntidadGubernamental>> LeerTodasAsync(CancellationToken cancellationToken)
    {
        await _semaforo.WaitAsync(cancellationToken);

        try
        {
            return await LeerSinBloqueoAsync(cancellationToken);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private async Task<IReadOnlyList<EntidadGubernamental>> LeerSinBloqueoAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_rutaArchivo))
        {
            return [];
        }

        string[] lineas = await File.ReadAllLinesAsync(_rutaArchivo, cancellationToken);

        List<EntidadGubernamental> entidades = [];

        foreach (string linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                continue;
            }

            entidades.Add(DesdeLinea(linea));
        }

        return entidades;
    }

    private async Task EscribirTodasAsync(IReadOnlyList<EntidadGubernamental> entidades, CancellationToken cancellationToken)
    {
        string? directorio = Path.GetDirectoryName(_rutaArchivo);

        if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        string archivoTemporal = $"{_rutaArchivo}.tmp";

        await using (FileStream flujo = File.Create(archivoTemporal))
        await using (StreamWriter escritor = new(flujo))
        {
            foreach (EntidadGubernamental entidad in entidades)
            {
                await escritor.WriteLineAsync(ALinea(entidad));
            }
        }

        File.Move(archivoTemporal, _rutaArchivo, overwrite: true);
    }

    private static string ALinea(EntidadGubernamental entidad) => string.Join(
        SEPARADOR_CAMPO,
        entidad.Id.ToString(CultureInfo.InvariantCulture),
        entidad.Nombre,
        entidad.Categoria,
        entidad.PoderDelEstado,
        entidad.Sector,
        entidad.Activo.ToString(CultureInfo.InvariantCulture));

    private static EntidadGubernamental DesdeLinea(string linea)
    {
        string[] campos = linea.Split(SEPARADOR_CAMPO);

        if (campos.Length != CANTIDAD_CAMPOS)
        {
            throw new InvalidOperationException(
                $"Linea con formato invalido en el archivo de entidades gubernamentales: se esperaban " +
                $"{CANTIDAD_CAMPOS} campos separados por '{SEPARADOR_CAMPO}' y se encontraron {campos.Length}.");
        }

        return new EntidadGubernamental
        {
            Id = int.Parse(campos[0], CultureInfo.InvariantCulture),
            Nombre = campos[1],
            Categoria = campos[2],
            PoderDelEstado = campos[3],
            Sector = campos[4],
            Activo = bool.Parse(campos[5])
        };
    }
}
