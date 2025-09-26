using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevCapacityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyCalendarEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyCalendars",
                columns: table => new
                {
                    CompanyCalendarId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCalendars", x => x.CompanyCalendarId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyCalendarNonWorkingDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyCalendarId = table.Column<int>(type: "INTEGER", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCalendarNonWorkingDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyCalendarNonWorkingDays_CompanyCalendars_CompanyCalendarId",
                        column: x => x.CompanyCalendarId,
                        principalTable: "CompanyCalendars",
                        principalColumn: "CompanyCalendarId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCalendarNonWorkingDays_CompanyCalendarId",
                table: "CompanyCalendarNonWorkingDays",
                column: "CompanyCalendarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyCalendarNonWorkingDays");

            migrationBuilder.DropTable(
                name: "CompanyCalendars");
        }
    }
}
