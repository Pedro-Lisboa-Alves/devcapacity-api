using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevCapacityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInitiativesEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Initiatives",
                columns: table => new
                {
                    InitiativeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentInitiative = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PDs = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Initiatives", x => x.InitiativeId);
                    table.ForeignKey(
                        name: "FK_Initiatives_Initiatives_ParentInitiative",
                        column: x => x.ParentInitiative,
                        principalTable: "Initiatives",
                        principalColumn: "InitiativeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Initiative",
                table: "Tasks",
                column: "Initiative");

            migrationBuilder.CreateIndex(
                name: "IX_Initiatives_ParentInitiative",
                table: "Initiatives",
                column: "ParentInitiative");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Initiatives_Initiative",
                table: "Tasks",
                column: "Initiative",
                principalTable: "Initiatives",
                principalColumn: "InitiativeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Initiatives_Initiative",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "Initiatives");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Initiative",
                table: "Tasks");
        }
    }
}
