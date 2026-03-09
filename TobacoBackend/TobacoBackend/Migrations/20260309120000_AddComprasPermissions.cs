using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    public partial class AddComprasPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Compras_Visualizar",
                table: "PermisosEmpleados",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Compras_Crear",
                table: "PermisosEmpleados",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Compras_Editar",
                table: "PermisosEmpleados",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Compras_Eliminar",
                table: "PermisosEmpleados",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Compras_Visualizar", table: "PermisosEmpleados");
            migrationBuilder.DropColumn(name: "Compras_Crear", table: "PermisosEmpleados");
            migrationBuilder.DropColumn(name: "Compras_Editar", table: "PermisosEmpleados");
            migrationBuilder.DropColumn(name: "Compras_Eliminar", table: "PermisosEmpleados");
        }
    }
}
