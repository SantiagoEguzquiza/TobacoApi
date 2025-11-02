using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class DividirUsuarioIdEnCreadorYAsignado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Users_UsuarioId",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Ventas",
                newName: "UsuarioIdCreador");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_UsuarioId",
                table: "Ventas",
                newName: "IX_Ventas_UsuarioIdCreador");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioIdAsignado",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioIdAsignado",
                table: "Ventas",
                column: "UsuarioIdAsignado");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Users_UsuarioIdAsignado",
                table: "Ventas",
                column: "UsuarioIdAsignado",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Users_UsuarioIdCreador",
                table: "Ventas",
                column: "UsuarioIdCreador",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Users_UsuarioIdAsignado",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Users_UsuarioIdCreador",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_UsuarioIdAsignado",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "UsuarioIdAsignado",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "UsuarioIdCreador",
                table: "Ventas",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_UsuarioIdCreador",
                table: "Ventas",
                newName: "IX_Ventas_UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Users_UsuarioId",
                table: "Ventas",
                column: "UsuarioId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
