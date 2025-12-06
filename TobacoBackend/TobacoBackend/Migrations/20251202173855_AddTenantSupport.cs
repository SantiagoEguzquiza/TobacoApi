using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RecorridosProgramados_VendedorId_DiaSemana_ClienteId",
                table: "RecorridosProgramados");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuantityPrices_ProductId_Quantity",
                table: "ProductQuantityPrices");

            migrationBuilder.DropIndex(
                name: "IX_PreciosEspeciales_ClienteId_ProductoId",
                table: "PreciosEspeciales");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Ventas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "RecorridosProgramados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ProductQuantityPrices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ProductosAFavor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PreciosEspeciales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PermisosEmpleados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Categorias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Asistencias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Abonos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            // Crear tenant por defecto para datos existentes
            migrationBuilder.Sql(@"
                INSERT INTO Tenants (Nombre, Descripcion, IsActive, CreatedAt)
                VALUES ('Tenant Por Defecto', 'Tenant creado automáticamente para migrar datos existentes', 1, GETUTCDATE());
            ");

            // Asignar el tenant por defecto (Id = 1) a todos los registros existentes
            migrationBuilder.Sql(@"
                UPDATE Users SET TenantId = 1 WHERE TenantId = 0;
                UPDATE Clientes SET TenantId = 1 WHERE TenantId = 0;
                UPDATE Productos SET TenantId = 1 WHERE TenantId = 0;
                UPDATE Categorias SET TenantId = 1 WHERE TenantId = 0;
                UPDATE Ventas SET TenantId = 1 WHERE TenantId = 0;
                UPDATE Abonos SET TenantId = 1 WHERE TenantId = 0;
                UPDATE PreciosEspeciales SET TenantId = 1 WHERE TenantId = 0;
                UPDATE ProductosAFavor SET TenantId = 1 WHERE TenantId = 0;
                UPDATE Asistencias SET TenantId = 1 WHERE TenantId = 0;
                UPDATE RecorridosProgramados SET TenantId = 1 WHERE TenantId = 0;
                UPDATE PermisosEmpleados SET TenantId = 1 WHERE TenantId = 0;
                UPDATE ProductQuantityPrices SET TenantId = 1 WHERE TenantId = 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_TenantId",
                table: "Ventas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_UserName",
                table: "Users",
                columns: new[] { "TenantId", "UserName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecorridosProgramados_TenantId_VendedorId_DiaSemana_ClienteId",
                table: "RecorridosProgramados",
                columns: new[] { "TenantId", "VendedorId", "DiaSemana", "ClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecorridosProgramados_VendedorId",
                table: "RecorridosProgramados",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantityPrices_ProductId",
                table: "ProductQuantityPrices",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantityPrices_TenantId_ProductId_Quantity",
                table: "ProductQuantityPrices",
                columns: new[] { "TenantId", "ProductId", "Quantity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAFavor_TenantId",
                table: "ProductosAFavor",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TenantId",
                table: "Productos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PreciosEspeciales_ClienteId",
                table: "PreciosEspeciales",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PreciosEspeciales_TenantId_ClienteId_ProductoId",
                table: "PreciosEspeciales",
                columns: new[] { "TenantId", "ClienteId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermisosEmpleados_TenantId",
                table: "PermisosEmpleados",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TenantId",
                table: "Clientes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_TenantId_Nombre",
                table: "Categorias",
                columns: new[] { "TenantId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_TenantId",
                table: "Asistencias",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Abonos_TenantId",
                table: "Abonos",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Abonos_Tenants_TenantId",
                table: "Abonos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Tenants_TenantId",
                table: "Asistencias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Tenants_TenantId",
                table: "Categorias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Tenants_TenantId",
                table: "Clientes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PermisosEmpleados_Tenants_TenantId",
                table: "PermisosEmpleados",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreciosEspeciales_Tenants_TenantId",
                table: "PreciosEspeciales",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Tenants_TenantId",
                table: "Productos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductosAFavor_Tenants_TenantId",
                table: "ProductosAFavor",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantityPrices_Tenants_TenantId",
                table: "ProductQuantityPrices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecorridosProgramados_Tenants_TenantId",
                table: "RecorridosProgramados",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Tenants_TenantId",
                table: "Ventas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Abonos_Tenants_TenantId",
                table: "Abonos");

            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Tenants_TenantId",
                table: "Asistencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Tenants_TenantId",
                table: "Categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Tenants_TenantId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_PermisosEmpleados_Tenants_TenantId",
                table: "PermisosEmpleados");

            migrationBuilder.DropForeignKey(
                name: "FK_PreciosEspeciales_Tenants_TenantId",
                table: "PreciosEspeciales");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Tenants_TenantId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductosAFavor_Tenants_TenantId",
                table: "ProductosAFavor");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantityPrices_Tenants_TenantId",
                table: "ProductQuantityPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_RecorridosProgramados_Tenants_TenantId",
                table: "RecorridosProgramados");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Tenants_TenantId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_TenantId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_UserName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RecorridosProgramados_TenantId_VendedorId_DiaSemana_ClienteId",
                table: "RecorridosProgramados");

            migrationBuilder.DropIndex(
                name: "IX_RecorridosProgramados_VendedorId",
                table: "RecorridosProgramados");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuantityPrices_ProductId",
                table: "ProductQuantityPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuantityPrices_TenantId_ProductId_Quantity",
                table: "ProductQuantityPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductosAFavor_TenantId",
                table: "ProductosAFavor");

            migrationBuilder.DropIndex(
                name: "IX_Productos_TenantId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_PreciosEspeciales_ClienteId",
                table: "PreciosEspeciales");

            migrationBuilder.DropIndex(
                name: "IX_PreciosEspeciales_TenantId_ClienteId_ProductoId",
                table: "PreciosEspeciales");

            migrationBuilder.DropIndex(
                name: "IX_PermisosEmpleados_TenantId",
                table: "PermisosEmpleados");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_TenantId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_TenantId_Nombre",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Asistencias_TenantId",
                table: "Asistencias");

            migrationBuilder.DropIndex(
                name: "IX_Abonos_TenantId",
                table: "Abonos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RecorridosProgramados");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductQuantityPrices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductosAFavor");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PreciosEspeciales");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PermisosEmpleados");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Abonos");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecorridosProgramados_VendedorId_DiaSemana_ClienteId",
                table: "RecorridosProgramados",
                columns: new[] { "VendedorId", "DiaSemana", "ClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantityPrices_ProductId_Quantity",
                table: "ProductQuantityPrices",
                columns: new[] { "ProductId", "Quantity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreciosEspeciales_ClienteId_ProductoId",
                table: "PreciosEspeciales",
                columns: new[] { "ClienteId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true);
        }
    }
}
