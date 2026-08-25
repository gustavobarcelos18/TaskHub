using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhaPrimeiraAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDatasAuditoriaTarefas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CONCLUIDA_EM",
                table: "TAREFAS",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CRIADA_EM",
                table: "TAREFAS",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MODIFICADA_EM",
                table: "TAREFAS",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SITUACAO_ALTERADA_EM",
                table: "TAREFAS",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE TAREFAS
                SET CRIADA_EM = CURRENT_TIMESTAMP,
                    SITUACAO_ALTERADA_EM = CURRENT_TIMESTAMP,
                    CONCLUIDA_EM = CASE
                        WHEN TRIM(SITUACAO) = 'Concluída'
                            THEN CURRENT_TIMESTAMP
                        ELSE NULL
                    END;
                """
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "CRIADA_EM",
                table: "TAREFAS",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SITUACAO_ALTERADA_EM",
                table: "TAREFAS",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CONCLUIDA_EM",
                table: "TAREFAS");

            migrationBuilder.DropColumn(
                name: "CRIADA_EM",
                table: "TAREFAS");

            migrationBuilder.DropColumn(
                name: "MODIFICADA_EM",
                table: "TAREFAS");

            migrationBuilder.DropColumn(
                name: "SITUACAO_ALTERADA_EM",
                table: "TAREFAS");
        }
    }
}
