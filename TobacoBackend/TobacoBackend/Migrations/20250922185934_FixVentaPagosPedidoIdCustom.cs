using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixVentaPagosPedidoIdCustom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, drop foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos");

            migrationBuilder.DropForeignKey(
                name: "FK_VentaPagos_Pedidos_VentaId",
                table: "VentaPagos");

            // Drop the index on VentaId
            migrationBuilder.DropIndex(
                name: "IX_VentaPagos_VentaId",
                table: "VentaPagos");

            // Update any NULL PedidoId values to use VentaId values
            migrationBuilder.Sql(@"
                UPDATE VentaPagos 
                SET PedidoId = VentaId 
                WHERE PedidoId IS NULL AND VentaId IS NOT NULL
            ");

            // Delete any records where both PedidoId and VentaId are NULL or invalid
            migrationBuilder.Sql(@"
                DELETE FROM VentaPagos 
                WHERE PedidoId IS NULL OR PedidoId NOT IN (SELECT Id FROM Pedidos)
            ");

            // Drop the VentaId column
            migrationBuilder.DropColumn(
                name: "VentaId",
                table: "VentaPagos");

            // Make PedidoId NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "PedidoId",
                table: "VentaPagos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Add the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos");

            migrationBuilder.AlterColumn<int>(
                name: "PedidoId",
                table: "VentaPagos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "VentaId",
                table: "VentaPagos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VentaPagos_VentaId",
                table: "VentaPagos",
                column: "VentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaPagos_Pedidos_VentaId",
                table: "VentaPagos",
                column: "VentaId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}