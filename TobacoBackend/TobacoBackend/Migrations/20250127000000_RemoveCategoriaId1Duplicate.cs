using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoriaId1Duplicate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar el índice si existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Productos_CategoriaId1' AND object_id = OBJECT_ID('Productos'))
                BEGIN
                    DROP INDEX [IX_Productos_CategoriaId1] ON [Productos];
                END
            ");

            // Eliminar la foreign key si existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Productos_Categorias_CategoriaId1' AND parent_object_id = OBJECT_ID('Productos'))
                BEGIN
                    ALTER TABLE [Productos] DROP CONSTRAINT [FK_Productos_Categorias_CategoriaId1];
                END
            ");

            // Eliminar la columna CategoriaId1 si existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'CategoriaId1')
                BEGIN
                    ALTER TABLE [Productos] DROP COLUMN [CategoriaId1];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recrear la columna (solo para poder revertir, aunque no debería ser necesario)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'CategoriaId1')
                BEGIN
                    ALTER TABLE [Productos] ADD [CategoriaId1] int NULL;
                END
            ");

            // Recrear el índice
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Productos_CategoriaId1' AND object_id = OBJECT_ID('Productos'))
                BEGIN
                    CREATE INDEX [IX_Productos_CategoriaId1] ON [Productos] ([CategoriaId1]);
                END
            ");

            // Recrear la foreign key
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Productos_Categorias_CategoriaId1' AND parent_object_id = OBJECT_ID('Productos'))
                BEGIN
                    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Categorias_CategoriaId1] 
                    FOREIGN KEY ([CategoriaId1]) REFERENCES [Categorias] ([Id]);
                END
            ");
        }
    }
}
