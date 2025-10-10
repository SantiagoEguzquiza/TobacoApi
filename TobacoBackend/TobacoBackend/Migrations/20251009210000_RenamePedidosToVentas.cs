using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class RenamePedidosToVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Eliminar foreign keys existentes antes de renombrar
            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProductos_Pedidos_PedidoId",
                table: "PedidosProductos");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProductos_Productos_ProductoId",
                table: "PedidosProductos");

            migrationBuilder.DropForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Users_UsuarioId",
                table: "Pedidos");

            // 2. Renombrar primary keys si es necesario
            migrationBuilder.DropPrimaryKey(
                name: "PK_PedidosProductos",
                table: "PedidosProductos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pedidos",
                table: "Pedidos");

            // 3. Renombrar tabla PedidosProductos a VentasProductos
            migrationBuilder.RenameTable(
                name: "PedidosProductos",
                newName: "VentasProductos");

            // 4. Renombrar columna PedidoId a VentaId en VentasProductos
            migrationBuilder.RenameColumn(
                name: "PedidoId",
                table: "VentasProductos",
                newName: "VentaId");

            // 5. Renombrar columna PedidoId a VentaId en VentaPagos
            migrationBuilder.RenameColumn(
                name: "PedidoId",
                table: "VentaPagos",
                newName: "VentaId");

            // 6. Renombrar índice en VentaPagos si existe
            migrationBuilder.RenameIndex(
                name: "IX_VentaPagos_PedidoId",
                table: "VentaPagos",
                newName: "IX_VentaPagos_VentaId");

            // 7. Renombrar tabla Pedidos a Ventas
            migrationBuilder.RenameTable(
                name: "Pedidos",
                newName: "Ventas");

            // 8. Recrear primary keys con nuevos nombres
            migrationBuilder.AddPrimaryKey(
                name: "PK_Ventas",
                table: "Ventas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VentasProductos",
                table: "VentasProductos",
                columns: new[] { "VentaId", "ProductoId" });

            // 9. Recrear foreign keys con nuevos nombres
            migrationBuilder.AddForeignKey(
                name: "FK_VentasProductos_Ventas_VentaId",
                table: "VentasProductos",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VentasProductos_Productos_ProductoId",
                table: "VentasProductos",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VentaPagos_Ventas_VentaId",
                table: "VentaPagos",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Users_UsuarioId",
                table: "Ventas",
                column: "UsuarioId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir en orden inverso
            
            // 1. Eliminar foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_VentasProductos_Ventas_VentaId",
                table: "VentasProductos");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasProductos_Productos_ProductoId",
                table: "VentasProductos");

            migrationBuilder.DropForeignKey(
                name: "FK_VentaPagos_Ventas_VentaId",
                table: "VentaPagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Users_UsuarioId",
                table: "Ventas");

            // 2. Eliminar primary keys
            migrationBuilder.DropPrimaryKey(
                name: "PK_Ventas",
                table: "Ventas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VentasProductos",
                table: "VentasProductos");

            // 3. Renombrar tabla Ventas a Pedidos
            migrationBuilder.RenameTable(
                name: "Ventas",
                newName: "Pedidos");

            // 4. Renombrar columnas
            migrationBuilder.RenameColumn(
                name: "VentaId",
                table: "VentaPagos",
                newName: "PedidoId");

            migrationBuilder.RenameColumn(
                name: "VentaId",
                table: "VentasProductos",
                newName: "PedidoId");

            // 5. Renombrar índice
            migrationBuilder.RenameIndex(
                name: "IX_VentaPagos_VentaId",
                table: "VentaPagos",
                newName: "IX_VentaPagos_PedidoId");

            // 6. Renombrar tabla VentasProductos a PedidosProductos
            migrationBuilder.RenameTable(
                name: "VentasProductos",
                newName: "PedidosProductos");

            // 7. Recrear primary keys originales
            migrationBuilder.AddPrimaryKey(
                name: "PK_Pedidos",
                table: "Pedidos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PedidosProductos",
                table: "PedidosProductos",
                columns: new[] { "PedidoId", "ProductoId" });

            // 8. Recrear foreign keys originales
            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProductos_Pedidos_PedidoId",
                table: "PedidosProductos",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProductos_Productos_ProductoId",
                table: "PedidosProductos",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VentaPagos_Pedidos_PedidoId",
                table: "VentaPagos",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Users_UsuarioId",
                table: "Pedidos",
                column: "UsuarioId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
