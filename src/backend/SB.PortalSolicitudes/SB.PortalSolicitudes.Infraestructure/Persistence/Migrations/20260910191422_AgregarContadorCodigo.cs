using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SB.PortalSolicitudes.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarContadorCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContadoresCodigo",
                columns: table => new
                {
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Ultimo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContadoresCodigo", x => x.Anio);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContadoresCodigo");
        }
    }
}
