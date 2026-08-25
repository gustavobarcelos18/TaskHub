using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhaPrimeiraAPI.Migrations
{
    public partial class CorrigirBancoExcluidaEm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EXCLUIDA_EM",
                table: "TAREFAS",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EXCLUIDA_EM",
                table: "TAREFAS");
        }
    }
}
