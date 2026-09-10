using Microsoft.EntityFrameworkCore;
using SB.PortalSolicitudes.Application.Abstractions.Persistence;

namespace SB.PortalSolicitudes.Infraestructure.Persistence;

public class PreparadorBaseDeDatos : IPreparadorBaseDeDatos
{
    private readonly PortalSolicitudesDbContext _contexto;

    public PreparadorBaseDeDatos(PortalSolicitudesDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task AplicarMigracionesPendientesAsync(CancellationToken cancellationToken = default) =>
        _contexto.Database.MigrateAsync(cancellationToken);
}
