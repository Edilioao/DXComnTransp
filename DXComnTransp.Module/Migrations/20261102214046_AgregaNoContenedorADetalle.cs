using Microsoft.EntityFrameworkCore.Migrations;

namespace DXComnTransp.Module.Migrations
{
    public partial class AgregaNoContenedorADetalle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
            name: "NoContenedor",
            table: "ComprasCombustibleDetalle",
            type: "nvarchar(11)",
            maxLength: 11,
            nullable: false,
            defaultValue: "CNTR0000001");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoContenedor",
                table: "ComprasCombustibleDetalle");
        }
    }
}
