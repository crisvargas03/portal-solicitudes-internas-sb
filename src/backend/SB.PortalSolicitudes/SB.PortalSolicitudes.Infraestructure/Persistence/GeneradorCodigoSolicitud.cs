using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SB.PortalSolicitudes.Application.Abstractions;

namespace SB.PortalSolicitudes.Infraestructure.Persistence;

/// <summary>
/// Implementacion sobre SQL Server de la generacion de codigo (ver ADR-0011). El
/// <c>UPDATE ... WITH (UPDLOCK, ROWLOCK)</c> serializa las creaciones concurrentes dentro
/// de un mismo año sin bloqueos explicitos en el codigo administrado. Para el primer
/// registro de un año nuevo, la fila del contador todavia no existe: el <c>INSERT</c>
/// puede chocar entre dos transacciones concurrentes, así que el que pierde la carrera
/// cae de vuelta al <c>UPDATE</c> dentro del mismo <c>TRY/CATCH</c> en lugar de fallar.
/// El indice unico de <c>Solicitud.Codigo</c> (ver <see cref="Configurations.SolicitudConfiguration"/>)
/// es el respaldo final ante cualquier caso no cubierto por este mecanismo.
/// </summary>
public class GeneradorCodigoSolicitud : IGeneradorCodigoSolicitud
{
    private const string FORMATO_CODIGO = "SOL-{0}-{1:D4}";

    private const string SQL_OBTENER_Y_ACTUALIZAR = """
        UPDATE ContadoresCodigo WITH (UPDLOCK, ROWLOCK)
        SET @siguiente = Ultimo = Ultimo + 1
        WHERE Anio = @anio;

        IF @@ROWCOUNT = 0
        BEGIN
            BEGIN TRY
                INSERT INTO ContadoresCodigo (Anio, Ultimo) VALUES (@anio, 1);
                SET @siguiente = 1;
            END TRY
            BEGIN CATCH
                UPDATE ContadoresCodigo WITH (UPDLOCK, ROWLOCK)
                SET @siguiente = Ultimo = Ultimo + 1
                WHERE Anio = @anio;
            END CATCH
        END
        """;

    private readonly PortalSolicitudesDbContext _contexto;

    public GeneradorCodigoSolicitud(PortalSolicitudesDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<string> GenerarAsync(int anio, CancellationToken cancellationToken = default)
    {
        int siguiente = await ObtenerYActualizarUltimoAsync(anio, cancellationToken);

        return string.Format(FORMATO_CODIGO, anio, siguiente);
    }

    private async Task<int> ObtenerYActualizarUltimoAsync(int anio, CancellationToken cancellationToken)
    {
        var conexion = (SqlConnection)_contexto.Database.GetDbConnection();

        if (conexion.State != ConnectionState.Open)
        {
            await conexion.OpenAsync(cancellationToken);
        }

        var transaccionActual = _contexto.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;

        await using SqlCommand comando = conexion.CreateCommand();
        comando.CommandText = SQL_OBTENER_Y_ACTUALIZAR;
        comando.Transaction = transaccionActual;
        comando.Parameters.Add(new SqlParameter("@anio", SqlDbType.Int) { Value = anio });

        SqlParameter parametroSiguiente = new("@siguiente", SqlDbType.Int) { Direction = ParameterDirection.Output };
        comando.Parameters.Add(parametroSiguiente);

        await comando.ExecuteNonQueryAsync(cancellationToken);

        return (int)parametroSiguiente.Value;
    }
}
