using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevCapacityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineerNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Engineers_Name",
                table: "Engineers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Engineers_Name",
                table: "Engineers");
        }
    }
}
