using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;
using SB.PortalSolicitudes.Infraestructure.Persistence;
using SB.PortalSolicitudes.Infraestructure.Persistence.Repositories;

namespace SB.PortalSolicitudes.Infraestructure;

/// <summary>
/// Registro de los servicios de infraestructura. La cadena de conexion se lee de
/// configuracion o variables de entorno.
/// </summary>
public static class InfraestructureExtension
{
    public const string NOMBRE_CADENA_CONEXION = "PortalSolicitudes";

    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios,
        IConfiguration configuracion)
    {
        string? cadenaConexion = configuracion.GetConnectionString(NOMBRE_CADENA_CONEXION);

        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                $"No se configuro la cadena de conexion '{NOMBRE_CADENA_CONEXION}'. " +
                $"Defina 'ConnectionStrings:{NOMBRE_CADENA_CONEXION}' en la configuracion " +
                $"o la variable de entorno 'ConnectionStrings__{NOMBRE_CADENA_CONEXION}'.");
        }

        servicios.AddDbContext<PortalSolicitudesDbContext>(opciones =>
            opciones.UseSqlServer(
                cadenaConexion,
                sqlServer => sqlServer.EnableRetryOnFailure()));

        servicios.AddScoped<IUnitOfWork, UnitOfWork>();

        servicios.AddScoped<IUsuarioRepository, UsuarioRepository>();
        servicios.AddScoped<ISolicitudRepository, SolicitudRepository>();
        servicios.AddScoped<IAreaRepository, AreaRepository>();
        servicios.AddScoped<ITipoSolicitudRepository, TipoSolicitudRepository>();
        servicios.AddScoped<IPrioridadRepository, PrioridadRepository>();
        servicios.AddScoped<IEstadoSolicitudRepository, EstadoSolicitudRepository>();
        servicios.AddScoped<ITransicionPermitidaRepository, TransicionPermitidaRepository>();
        servicios.AddScoped<IHistorialEstadoRepository, HistorialEstadoRepository>();
        servicios.AddScoped<IComentarioRepository, ComentarioRepository>();
        servicios.AddScoped<IAdjuntoRepository, AdjuntoRepository>();
        servicios.AddScoped<INotificacionRepository, NotificacionRepository>();

        return servicios;
    }
}
