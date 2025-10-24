using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevCapacityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskMaxResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxResources",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxResources",
                table: "Tasks");
        }
    }
}
