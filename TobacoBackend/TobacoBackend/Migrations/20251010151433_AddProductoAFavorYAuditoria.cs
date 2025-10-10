using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductoAFavorYAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaChequeo",
                table: "VentasProductos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "VentasProductos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nota",
                table: "VentasProductos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioChequeoId",
                table: "VentasProductos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductosAFavor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nota = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    VentaProductoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioRegistroId = table.Column<int>(type: "int", nullable: true),
                    Entregado = table.Column<bool>(type: "bit", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEntregaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosAFavor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosAFavor_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosAFavor_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosAFavor_Users_UsuarioEntregaId",
                        column: x => x.UsuarioEntregaId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductosAFavor_Users_UsuarioRegistroId",
                        column: x => x.UsuarioRegistroId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductosAFavor_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentasProductos_UsuarioChequeoId",
                table: "VentasProductos",
                column: "UsuarioChequeoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAFavor_ClienteId",
                table: "ProductosAFavor",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAFavor_ProductoId",
                table: "ProductosAFavor",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAFavor_UsuarioEntregaId",
                table: "ProductosAFavor",
                column: "UsuarioEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAFavor_UsuarioRegistroId",
                table: "ProductosAFavor",
                column: "UsuarioRegistroId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAFavor_VentaId",
                table: "ProductosAFavor",
                column: "VentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_VentasProductos_Users_UsuarioChequeoId",
                table: "VentasProductos",
                column: "UsuarioChequeoId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VentasProductos_Users_UsuarioChequeoId",
                table: "VentasProductos");

            migrationBuilder.DropTable(
                name: "ProductosAFavor");

            migrationBuilder.DropIndex(
                name: "IX_VentasProductos_UsuarioChequeoId",
                table: "VentasProductos");

            migrationBuilder.DropColumn(
                name: "FechaChequeo",
                table: "VentasProductos");

            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "VentasProductos");

            migrationBuilder.DropColumn(
                name: "Nota",
                table: "VentasProductos");

            migrationBuilder.DropColumn(
                name: "UsuarioChequeoId",
                table: "VentasProductos");
        }
    }
}
