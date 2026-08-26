using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoTarefas.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEtiquetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ETIQUETAS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NOME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NOME_NORMALIZADO = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ETIQUETAS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TAREFA_ETIQUETA",
                columns: table => new
                {
                    TAREFA_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    ETIQUETA_ID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAREFA_ETIQUETA", x => new { x.TAREFA_ID, x.ETIQUETA_ID });
                    table.ForeignKey(
                        name: "FK_TAREFA_ETIQUETA_ETIQUETAS_ETIQUETA_ID",
                        column: x => x.ETIQUETA_ID,
                        principalTable: "ETIQUETAS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TAREFA_ETIQUETA_TAREFAS_TAREFA_ID",
                        column: x => x.TAREFA_ID,
                        principalTable: "TAREFAS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ETIQUETAS_NOME_NORMALIZADO",
                table: "ETIQUETAS",
                column: "NOME_NORMALIZADO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TAREFA_ETIQUETA_ETIQUETA_ID",
                table: "TAREFA_ETIQUETA",
                column: "ETIQUETA_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TAREFA_ETIQUETA");

            migrationBuilder.DropTable(
                name: "ETIQUETAS");
        }
    }
}
