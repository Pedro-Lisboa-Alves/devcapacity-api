using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevCapacityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineerCalendarEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineerCalendars",
                columns: table => new
                {
                    EngineerCalendarId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EngineerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineerCalendars", x => x.EngineerCalendarId);
                    table.ForeignKey(
                        name: "FK_EngineerCalendars_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalTable: "Engineers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_EngineerCalendars_EngineerId",
                table: "EngineerCalendars",
                column: "EngineerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EngineerCalendarVacations_EngineerCalendarId",
                table: "EngineerCalendarVacations",
                column: "EngineerCalendarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerCalendarVacations");

            migrationBuilder.DropTable(
                name: "EngineerCalendars");
        }
    }
}
