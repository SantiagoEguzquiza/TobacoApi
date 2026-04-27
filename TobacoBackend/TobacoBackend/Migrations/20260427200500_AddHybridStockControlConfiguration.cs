using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddHybridStockControlConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Modo de control de stock por producto.
            // 0 = InheritTenant, 1 = ForceEnabled, 2 = ForceDisabled
            migrationBuilder.AddColumn<int>(
                name: "StockControlMode",
                table: "Productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Configuración global del tenant para controlar stock por defecto.
            migrationBuilder.AddColumn<bool>(
                name: "StockControlEnabledByDefault",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockControlMode",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockControlEnabledByDefault",
                table: "Tenants");
        }
    }
}
