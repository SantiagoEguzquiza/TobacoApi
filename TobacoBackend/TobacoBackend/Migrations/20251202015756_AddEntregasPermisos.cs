using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEntregasPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Entregas_ActualizarEstado",
                table: "PermisosEmpleados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Entregas_Visualizar",
                table: "PermisosEmpleados",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entregas_ActualizarEstado",
                table: "PermisosEmpleados");

            migrationBuilder.DropColumn(
                name: "Entregas_Visualizar",
                table: "PermisosEmpleados");
        }
    }
}
