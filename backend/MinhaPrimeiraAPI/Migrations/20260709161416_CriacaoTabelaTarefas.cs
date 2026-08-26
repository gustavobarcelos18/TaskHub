using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoTarefas.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoTabelaTarefas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TAREFAS",
                columns: table => new
                {
                    CODIGO = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DESCRICAO = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SITUACAO = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAREFAS", x => x.CODIGO);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TAREFAS");
        }
    }
}
