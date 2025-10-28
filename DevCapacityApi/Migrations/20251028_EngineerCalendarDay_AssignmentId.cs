using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

public partial class EngineerCalendarDay_AssignmentId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // add nullable AssignmentId column
        migrationBuilder.AddColumn<int>(
            name: "AssignmentId",
            table: "EngineerCalendarDay",
            type: "INTEGER",
            nullable: true);

        // create index for FK lookup
        migrationBuilder.CreateIndex(
            name: "IX_EngineerCalendarDay_AssignmentId",
            table: "EngineerCalendarDay",
            column: "AssignmentId");

        // add FK constraint to EngineerAssignment.AssignmentId (restrict on delete)
        migrationBuilder.AddForeignKey(
            name: "FK_EngineerCalendarDay_EngineerAssignment_AssignmentId",
            table: "EngineerCalendarDay",
            column: "AssignmentId",
            principalTable: "EngineerAssignment",
            principalColumn: "AssignmentId",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_EngineerCalendarDay_EngineerAssignment_AssignmentId",
            table: "EngineerCalendarDay");

        migrationBuilder.DropIndex(
            name: "IX_EngineerCalendarDay_AssignmentId",
            table: "EngineerCalendarDay");

        migrationBuilder.DropColumn(
            name: "AssignmentId",
            table: "EngineerCalendarDay");
    }
}