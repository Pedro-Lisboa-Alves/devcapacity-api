using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevCapacityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerCalendarVacations");

            migrationBuilder.CreateTable(
                name: "EngineerCalendarDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EngineerCalendarId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineerCalendarDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerCalendarDays_EngineerCalendars_EngineerCalendarId",
                        column: x => x.EngineerCalendarId,
                        principalTable: "EngineerCalendars",
                        principalColumn: "EngineerCalendarId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerCalendarDays_EngineerCalendarId",
                table: "EngineerCalendarDays",
                column: "EngineerCalendarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerCalendarDays");

            migrationBuilder.CreateTable(
                name: "EngineerCalendarVacations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EngineerCalendarId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineerCalendarVacations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerCalendarVacations_EngineerCalendars_EngineerCalendarId",
                        column: x => x.EngineerCalendarId,
                        principalTable: "EngineerCalendars",
                        principalColumn: "EngineerCalendarId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerCalendarVacations_EngineerCalendarId",
                table: "EngineerCalendarVacations",
                column: "EngineerCalendarId");
        }
    }
}
