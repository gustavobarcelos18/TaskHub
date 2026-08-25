using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhaPrimeiraAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPrioridadeEDataVencimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DATA_VENCIMENTO",
                table: "TAREFAS",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PRIORIDADE",
                table: "TAREFAS",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "Media");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DATA_VENCIMENTO",
                table: "TAREFAS");

            migrationBuilder.DropColumn(
                name: "PRIORIDADE",
                table: "TAREFAS");
        }
    }
}
