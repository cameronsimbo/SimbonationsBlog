using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHybridCatalogAndVoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GenerationGuidance",
                table: "Topics",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyConcepts",
                table: "Topics",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenerationContext",
                table: "Lessons",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasGeneratedExercises",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DownvoteCount",
                table: "Exercises",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpvoteCount",
                table: "Exercises",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ExerciseVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsUpvote = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseVotes_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseVotes_ExerciseId_UserId",
                table: "ExerciseVotes",
                columns: new[] { "ExerciseId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseVotes");

            migrationBuilder.DropColumn(
                name: "GenerationGuidance",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "KeyConcepts",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "GenerationContext",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "HasGeneratedExercises",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "DownvoteCount",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UpvoteCount",
                table: "Exercises");
        }
    }
}
