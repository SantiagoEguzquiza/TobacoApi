using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPreciosEspecialesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreciosEspeciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreciosEspeciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreciosEspeciales_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreciosEspeciales_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreciosEspeciales_ClienteId_ProductoId",
                table: "PreciosEspeciales",
                columns: new[] { "ClienteId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreciosEspeciales_ClienteId",
                table: "PreciosEspeciales",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PreciosEspeciales_ProductoId",
                table: "PreciosEspeciales",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreciosEspeciales");
        }
    }
}
