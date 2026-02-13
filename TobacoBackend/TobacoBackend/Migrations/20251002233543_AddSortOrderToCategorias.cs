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
            // Verificar si la columna SortOrder ya existe antes de crearla
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Categorias') AND name = 'SortOrder')
                BEGIN
                    ALTER TABLE [Categorias] ADD [SortOrder] int NOT NULL DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Verificar si la columna SortOrder existe antes de eliminarla
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Categorias') AND name = 'SortOrder')
                BEGIN
                    ALTER TABLE [Categorias] DROP COLUMN [SortOrder];
                END
            ");
        }
    }
}
