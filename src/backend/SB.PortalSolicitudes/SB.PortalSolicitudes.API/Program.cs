using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using SB.PortalSolicitudes.API.Autenticacion;
using SB.PortalSolicitudes.API.Common;
using SB.PortalSolicitudes.API.Middleware;
using SB.PortalSolicitudes.Application;
using SB.PortalSolicitudes.Application.Abstractions;
using SB.PortalSolicitudes.Application.Abstractions.Autenticacion;
using SB.PortalSolicitudes.Infraestructure;
using Serilog;

const int LONGITUD_MINIMA_CLAVE_JWT_EN_BYTES = 32;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((contexto, configuracion) => configuracion.ReadFrom.Configuration(contexto.Configuration));

    builder.Services.AddHttpContextAccessor();

    // JsonStringEnumConverter: los enums (RolUsuario, EstadoNotificacion, CanalNotificacion)
    // viajan como su nombre ("Administrador") en vez de su valor entero subyacente, tanto en
    // el cuerpo de la peticion/respuesta como en el esquema que genera Swagger — consistente
    // con como ya se exponen manualmente en UsuarioResumenDto.Rol (ver ProveedorTokensJwt,
    // que ya hace lo mismo a mano con el claim de rol del JWT).
    builder.Services.AddControllers()
        .AddJsonOptions(opciones => opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddEndpointsApiExplorer();

    // El [ApiController] responde 400 por su cuenta ante un fallo de model-binding (JSON
    // malformado, tipos que no calzan) antes de que el comando/consulta llegue a LiteBus:
    // sin este override usaria su ValidationProblemDetails por defecto en vez del sobre
    // RespuestaApi que usa el resto de la Api (ver ADR-0010, amendada).
    builder.Services.Configure<ApiBehaviorOptions>(opciones =>
    {
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            Dictionary<string, string[]> errores = contexto.ModelState
                .Where(entrada => entrada.Value?.Errors.Count > 0)
                .ToDictionary(
                    entrada => entrada.Key,
                    entrada => entrada.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            RespuestaApi cuerpo = RespuestaApi.CrearError(new ErrorApiDto
            {
                Codigo = "Validacion",
                Detalle = "Uno o mas campos no son validos.",
                Errores = errores
            });

            return new BadRequestObjectResult(cuerpo);
        };
    });

    builder.Services.AddSwaggerGen(opciones =>
    {
        opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Token JWT obtenido en POST /api/auth/login. Formato: Bearer {token}"
        });

        opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddProblemDetails();

    string[] origenesPermitidos = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];

    const string POLITICA_CORS = "PortalSolicitudes";

    builder.Services.AddCors(opciones =>
    {
        opciones.AddPolicy(POLITICA_CORS, politica =>
        {
            politica.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod();
        });
    });

    OpcionesJwt opcionesJwt =
        builder.Configuration.GetSection(OpcionesJwt.SECCION).Get<OpcionesJwt>() ?? new OpcionesJwt();

    if (string.IsNullOrWhiteSpace(opcionesJwt.ClaveSecreta)
        || Encoding.UTF8.GetByteCount(opcionesJwt.ClaveSecreta) < LONGITUD_MINIMA_CLAVE_JWT_EN_BYTES)
    {
        throw new InvalidOperationException(
            $"No se configuro 'Jwt:ClaveSecreta' (o mide menos de {LONGITUD_MINIMA_CLAVE_JWT_EN_BYTES} bytes). " +
            "Definala en la configuracion o en la variable de entorno 'Jwt__ClaveSecreta'.");
    }

    builder.Services
        .AddAuthentication(opciones =>
        {
            opciones.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opciones.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opciones =>
        {
            opciones.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = opcionesJwt.Emisor,
                ValidateAudience = true,
                ValidAudience = opcionesJwt.Audiencia,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcionesJwt.ClaveSecreta)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddExceptionHandler<ManejadorExcepcionValidacion>();
    builder.Services.AddExceptionHandler<ManejadorExcepcionGlobal>();

    builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
    builder.Services.AddScoped<IEntornoEjecucion, EntornoEjecucion>();

    builder.Services.AgregarAplicacion();
    builder.Services.AgregarInfraestructura(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();

    // EnrichDiagnosticContext corre al final de la peticion, leyendo el HttpContext ya
    // autenticado: es lo que permite que la linea-resumen de UseSerilogRequestLogging
    // lleve UsuarioId/Rol. MiddlewareContextoUsuario (mas abajo) cubre las lineas de log
    // emitidas dentro de la peticion (handlers, comportamientos de LiteBus) via LogContext
    // — ese scope se cierra antes de que la linea-resumen se emita, asi que no alcanza.
    app.UseSerilogRequestLogging(opciones =>
    {
        opciones.EnrichDiagnosticContext = (contexto, httpContext) =>
        {
            contexto.Set("UsuarioId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            contexto.Set("Rol", httpContext.User.FindFirstValue(ClaimTypes.Role));
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseCors(POLITICA_CORS);

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<MiddlewareContextoUsuario>();

    app.MapControllers();

    app.Run();
}
catch (Exception excepcion) when (excepcion is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    // HostAbortedException es el mecanismo con el que las herramientas de diseño de EF Core
    // (dotnet ef migrations/database update) detienen el host tras construirlo, para
    // extraer el DbContext sin ejecutar la aplicacion: no es un fallo real de arranque.
    Log.Fatal(excepcion, "La aplicacion no pudo iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
