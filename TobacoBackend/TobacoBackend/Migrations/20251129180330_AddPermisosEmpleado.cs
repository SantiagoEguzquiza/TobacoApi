using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPermisosEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermisosEmpleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Productos_Visualizar = table.Column<bool>(type: "bit", nullable: false),
                    Productos_Crear = table.Column<bool>(type: "bit", nullable: false),
                    Productos_Editar = table.Column<bool>(type: "bit", nullable: false),
                    Productos_Eliminar = table.Column<bool>(type: "bit", nullable: false),
                    Clientes_Visualizar = table.Column<bool>(type: "bit", nullable: false),
                    Clientes_Crear = table.Column<bool>(type: "bit", nullable: false),
                    Clientes_Editar = table.Column<bool>(type: "bit", nullable: false),
                    Clientes_Eliminar = table.Column<bool>(type: "bit", nullable: false),
                    Ventas_Visualizar = table.Column<bool>(type: "bit", nullable: false),
                    Ventas_Crear = table.Column<bool>(type: "bit", nullable: false),
                    Ventas_EditarBorrador = table.Column<bool>(type: "bit", nullable: false),
                    Ventas_Eliminar = table.Column<bool>(type: "bit", nullable: false),
                    CuentaCorriente_Visualizar = table.Column<bool>(type: "bit", nullable: false),
                    CuentaCorriente_RegistrarAbonos = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisosEmpleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermisosEmpleados_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermisosEmpleados_UserId",
                table: "PermisosEmpleados",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermisosEmpleados");
        }
    }
}
