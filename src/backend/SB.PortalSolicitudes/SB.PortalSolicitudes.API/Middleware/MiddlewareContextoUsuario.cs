using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using Serilog.Context;

namespace SB.PortalSolicitudes.API.Middleware;

/// <summary>Empuja <c>UsuarioId</c>/<c>Rol</c> al <c>LogContext</c> de Serilog para que toda linea de log dentro de la peticion los lleve.</summary>
public class MiddlewareContextoUsuario
{
    private readonly RequestDelegate _siguiente;

    public MiddlewareContextoUsuario(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public async Task InvokeAsync(HttpContext contexto, IUsuarioActual usuarioActual)
    {
        using (LogContext.PushProperty("UsuarioId", usuarioActual.Id))
        using (LogContext.PushProperty("Rol", usuarioActual.Rol))
        {
            await _siguiente(contexto);
        }
    }
}
