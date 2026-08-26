using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoTarefas.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHistoricoTarefas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HISTORICO_TAREFAS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TAREFA_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    TIPO = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CAMPO = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VALOR_ANTERIOR = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    VALOR_NOVO = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HISTORICO_TAREFAS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HISTORICO_TAREFAS_TAREFAS_TAREFA_ID",
                        column: x => x.TAREFA_ID,
                        principalTable: "TAREFAS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HISTORICO_TAREFAS_TAREFA_ID_CRIADO_EM",
                table: "HISTORICO_TAREFAS",
                columns: new[] { "TAREFA_ID", "CRIADO_EM" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HISTORICO_TAREFAS");
        }
    }
}
