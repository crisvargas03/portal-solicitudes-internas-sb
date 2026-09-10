using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsFinal = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosSolicitud", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prioridades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prioridades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposSolicitud", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransicionesPermitidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstadoOrigenId = table.Column<int>(type: "int", nullable: false),
                    EstadoDestinoId = table.Column<int>(type: "int", nullable: false),
                    RequiereComentario = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransicionesPermitidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransicionesPermitidas_EstadosSolicitud_EstadoDestinoId",
                        column: x => x.EstadoDestinoId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransicionesPermitidas_EstadosSolicitud_EstadoOrigenId",
                        column: x => x.EstadoOrigenId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCompromiso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrioridadId = table.Column<int>(type: "int", nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    TipoSolicitudId = table.Column<int>(type: "int", nullable: false),
                    UsuarioSolicitanteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAsignadoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_EstadosSolicitud_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Prioridades_PrioridadId",
                        column: x => x.PrioridadId,
                        principalTable: "Prioridades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_TiposSolicitud_TipoSolicitudId",
                        column: x => x.TipoSolicitudId,
                        principalTable: "TiposSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_UsuarioAsignadoId",
                        column: x => x.UsuarioAsignadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_UsuarioSolicitanteId",
                        column: x => x.UsuarioSolicitanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransicionesPermitidasRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransicionPermitidaId = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransicionesPermitidasRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransicionesPermitidasRoles_TransicionesPermitidas_TransicionPermitidaId",
                        column: x => x.TransicionPermitidaId,
                        principalTable: "TransicionesPermitidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adjuntos_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adjuntos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EsInterno = table.Column<bool>(type: "bit", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentarios_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialEstados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    EstadoAnteriorId = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEstados_EstadosSolicitud_EstadoAnteriorId",
                        column: x => x.EstadoAnteriorId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstados_EstadosSolicitud_EstadoNuevoId",
                        column: x => x.EstadoNuevoId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstados_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialEstados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    UsuarioDestinoId = table.Column<int>(type: "int", nullable: false),
                    Canal = table.Column<int>(type: "int", nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioDestinoId",
                        column: x => x.UsuarioDestinoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Tecnología de la Información" },
                    { 2, true, "Supervisión Bancaria" },
                    { 3, true, "Recursos Humanos" },
                    { 4, true, "Administración y Finanzas" },
                    { 5, true, "Consultoría Jurídica" }
                });

            migrationBuilder.InsertData(
                table: "EstadosSolicitud",
                columns: new[] { "Id", "Activo", "Codigo", "EsFinal", "Nombre", "Orden" },
                values: new object[,]
                {
                    { 1, true, "REGISTRADA", false, "Registrada", 1 },
                    { 2, true, "EN_ANALISIS", false, "En análisis", 2 },
                    { 3, true, "EN_PROGRESO", false, "En progreso", 3 },
                    { 4, true, "EN_ESPERA_SOLICITANTE", false, "En espera del solicitante", 4 },
                    { 5, true, "RESUELTA", false, "Resuelta", 5 },
                    { 6, true, "CERRADA", true, "Cerrada", 6 }
                });

            migrationBuilder.InsertData(
                table: "Prioridades",
                columns: new[] { "Id", "Activo", "Nivel", "Nombre" },
                values: new object[,]
                {
                    { 1, true, 1, "Baja" },
                    { 2, true, 2, "Media" },
                    { 3, true, 3, "Alta" },
                    { 4, true, 4, "Crítica" }
                });

            migrationBuilder.InsertData(
                table: "TiposSolicitud",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Fallas de equipos, red o servicios de tecnología.", "Soporte técnico" },
                    { 2, true, "Altas, bajas y cambios de permisos en aplicaciones internas.", "Acceso a sistemas" },
                    { 3, true, "Solicitud, reemplazo o traslado de equipos.", "Equipo y hardware" },
                    { 4, true, "Instalaciones, licencias y nuevas funcionalidades.", "Requerimiento de software" },
                    { 5, true, "Dudas y orientación que no encajan en otro tipo.", "Consulta general" }
                });

            migrationBuilder.InsertData(
                table: "TransicionesPermitidas",
                columns: new[] { "Id", "Activo", "EstadoDestinoId", "EstadoOrigenId", "RequiereComentario" },
                values: new object[,]
                {
                    { 1, true, 2, 1, false },
                    { 2, true, 3, 2, false },
                    { 3, true, 4, 3, true },
                    { 4, true, 3, 4, false },
                    { 5, true, 5, 3, true },
                    { 6, true, 3, 5, true },
                    { 7, true, 6, 5, false },
                    { 8, true, 2, 6, true }
                });

            migrationBuilder.InsertData(
                table: "TransicionesPermitidasRoles",
                columns: new[] { "Id", "Rol", "TransicionPermitidaId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 1, 2 },
                    { 4, 2, 2 },
                    { 5, 1, 3 },
                    { 6, 2, 3 },
                    { 7, 1, 4 },
                    { 8, 2, 4 },
                    { 9, 3, 4 },
                    { 10, 1, 5 },
                    { 11, 2, 5 },
                    { 12, 1, 6 },
                    { 13, 2, 6 },
                    { 14, 1, 7 },
                    { 15, 2, 7 },
                    { 16, 3, 7 },
                    { 17, 1, 8 },
                    { 18, 2, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adjuntos_SolicitudId",
                table: "Adjuntos",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Adjuntos_UsuarioId",
                table: "Adjuntos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Nombre",
                table: "Areas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_SolicitudId_EsInterno",
                table: "Comentarios",
                columns: new[] { "SolicitudId", "EsInterno" });

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadosSolicitud_Codigo",
                table: "EstadosSolicitud",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadosSolicitud_Orden",
                table: "EstadosSolicitud",
                column: "Orden",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstados_EstadoAnteriorId",
                table: "HistorialEstados",
                column: "EstadoAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstados_EstadoNuevoId",
                table: "HistorialEstados",
                column: "EstadoNuevoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstados_SolicitudId_EstadoNuevoId",
                table: "HistorialEstados",
                columns: new[] { "SolicitudId", "EstadoNuevoId" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstados_UsuarioId",
                table: "HistorialEstados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_Estado",
                table: "Notificaciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_SolicitudId",
                table: "Notificaciones",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioDestinoId",
                table: "Notificaciones",
                column: "UsuarioDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Prioridades_Nivel",
                table: "Prioridades",
                column: "Nivel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prioridades_Nombre",
                table: "Prioridades",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_AreaId",
                table: "Solicitudes",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_Codigo",
                table: "Solicitudes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_EstadoId",
                table: "Solicitudes",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_FechaCompromiso",
                table: "Solicitudes",
                column: "FechaCompromiso");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_FechaCreacion",
                table: "Solicitudes",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_PrioridadId",
                table: "Solicitudes",
                column: "PrioridadId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TipoSolicitudId",
                table: "Solicitudes",
                column: "TipoSolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_UsuarioAsignadoId",
                table: "Solicitudes",
                column: "UsuarioAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_UsuarioSolicitanteId",
                table: "Solicitudes",
                column: "UsuarioSolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposSolicitud_Nombre",
                table: "TiposSolicitud",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransicionesPermitidas_EstadoDestinoId",
                table: "TransicionesPermitidas",
                column: "EstadoDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransicionesPermitidas_EstadoOrigenId_EstadoDestinoId",
                table: "TransicionesPermitidas",
                columns: new[] { "EstadoOrigenId", "EstadoDestinoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransicionesPermitidasRoles_TransicionPermitidaId_Rol",
                table: "TransicionesPermitidasRoles",
                columns: new[] { "TransicionPermitidaId", "Rol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adjuntos");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "HistorialEstados");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "TransicionesPermitidasRoles");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "TransicionesPermitidas");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Prioridades");

            migrationBuilder.DropTable(
                name: "TiposSolicitud");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "EstadosSolicitud");
        }
    }
}
