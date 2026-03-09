using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    public partial class AddNumeroVentaToVentas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroVenta",
                table: "Ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                WITH ranked AS (
                    SELECT ""Id"", ""TenantId"",
                        ROW_NUMBER() OVER (PARTITION BY ""TenantId"" ORDER BY ""Fecha"", ""Id"") as rn
                    FROM ""Ventas""
                )
                UPDATE ""Ventas"" v
                SET ""NumeroVenta"" = r.rn
                FROM ranked r
                WHERE v.""Id"" = r.""Id"";
            ");

            migrationBuilder.AlterColumn<int>(
                name: "NumeroVenta",
                table: "Ventas",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_TenantId_NumeroVenta",
                table: "Ventas",
                columns: new[] { "TenantId", "NumeroVenta" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ventas_TenantId_NumeroVenta",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "NumeroVenta",
                table: "Ventas");
        }
    }
}
