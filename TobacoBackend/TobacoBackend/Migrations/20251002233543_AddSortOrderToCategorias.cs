using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSortOrderToCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Categorias",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Categorias");
        }
    }
}
