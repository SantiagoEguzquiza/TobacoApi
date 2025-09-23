using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePedidoVentaPagosRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PedidoId",
                table: "VentaPagos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaPagos_PedidoId",
                table: "VentaPagos",
                column: "PedidoId");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos");

            migrationBuilder.DropIndex(
                name: "IX_VentaPagos_PedidoId",
                table: "VentaPagos");

            migrationBuilder.DropColumn(
                name: "PedidoId",
                table: "VentaPagos");
        }
    }
}
