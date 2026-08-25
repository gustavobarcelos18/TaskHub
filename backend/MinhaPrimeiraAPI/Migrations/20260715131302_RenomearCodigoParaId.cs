using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhaPrimeiraAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenomearCodigoParaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CODIGO",
                table: "TAREFAS",
                newName: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "TAREFAS",
                newName: "CODIGO");
        }
    }
}
