using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TobacoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ambas columnas ya existen en la base de datos
            // Esta migración solo se registra en el historial
            
            // migrationBuilder.AddColumn<bool>(
            //     name: "Entregado",
            //     table: "VentasProductos",
            //     type: "bit",
            //     nullable: false,
            //     defaultValue: false);

            // migrationBuilder.AddColumn<int>(
            //     name: "EstadoEntrega",
            //     table: "Ventas",
            //     type: "int",
            //     nullable: false,
            //     defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se eliminan las columnas ya que no fueron creadas por esta migración
            // migrationBuilder.DropColumn(
            //     name: "Entregado",
            //     table: "VentasProductos");

            // migrationBuilder.DropColumn(
            //     name: "EstadoEntrega",
            //     table: "Ventas");
        }
    }
}
