using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFSRSFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Difficulty",
                table: "ReviewItems",
                type: "float",
                nullable: false,
                defaultValue: 5.0);

            migrationBuilder.AddColumn<double>(
                name: "Stability",
                table: "ReviewItems",
                type: "float",
                nullable: false,
                defaultValue: 1.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "ReviewItems");

            migrationBuilder.DropColumn(
                name: "Stability",
                table: "ReviewItems");
        }
    }
}
