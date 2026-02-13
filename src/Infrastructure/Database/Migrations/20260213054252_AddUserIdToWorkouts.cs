using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToWorkouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutActivities_Workouts_WorkoutId",
                table: "WorkoutActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutActivities_WorkoutId",
                table: "WorkoutActivities");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Workouts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkoutUserId",
                table: "WorkoutActivities",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts",
                columns: new[] { "Id", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId",
                table: "Workouts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutActivities_WorkoutId_WorkoutUserId",
                table: "WorkoutActivities",
                columns: new[] { "WorkoutId", "WorkoutUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutActivities_Workouts_WorkoutId_WorkoutUserId",
                table: "WorkoutActivities",
                columns: new[] { "WorkoutId", "WorkoutUserId" },
                principalTable: "Workouts",
                principalColumns: new[] { "Id", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutActivities_Workouts_WorkoutId_WorkoutUserId",
                table: "WorkoutActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserId",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutActivities_WorkoutId_WorkoutUserId",
                table: "WorkoutActivities");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "WorkoutUserId",
                table: "WorkoutActivities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutActivities_WorkoutId",
                table: "WorkoutActivities",
                column: "WorkoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutActivities_Workouts_WorkoutId",
                table: "WorkoutActivities",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
