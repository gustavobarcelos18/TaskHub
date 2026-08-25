using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhaPrimeiraAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarProjetos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PROJETO_ID",
                table: "TAREFAS",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PROJETOS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NOME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NOME_NORMALIZADO = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJETOS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TAREFAS_PROJETO_ID",
                table: "TAREFAS",
                column: "PROJETO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PROJETOS_NOME_NORMALIZADO",
                table: "PROJETOS",
                column: "NOME_NORMALIZADO",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TAREFAS_PROJETOS_PROJETO_ID",
                table: "TAREFAS",
                column: "PROJETO_ID",
                principalTable: "PROJETOS",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TAREFAS_PROJETOS_PROJETO_ID",
                table: "TAREFAS");

            migrationBuilder.DropTable(
                name: "PROJETOS");

            migrationBuilder.DropIndex(
                name: "IX_TAREFAS_PROJETO_ID",
                table: "TAREFAS");

            migrationBuilder.DropColumn(
                name: "PROJETO_ID",
                table: "TAREFAS");
        }
    }
}
