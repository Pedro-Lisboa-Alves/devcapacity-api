using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevCapacityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTasksEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Initiative = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PDs = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerAssignments_TaskId",
                table: "EngineerAssignments",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_EngineerAssignments_Tasks_TaskId",
                table: "EngineerAssignments",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngineerAssignments_Tasks_TaskId",
                table: "EngineerAssignments");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_EngineerAssignments_TaskId",
                table: "EngineerAssignments");
        }
    }
}
