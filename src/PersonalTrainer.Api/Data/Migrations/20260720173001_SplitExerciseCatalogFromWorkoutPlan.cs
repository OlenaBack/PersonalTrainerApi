using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalTrainer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitExerciseCatalogFromWorkoutPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_WorkoutPlans_WorkoutPlanId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_TrainerId",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Reps",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Sets",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "WorkoutPlanId",
                table: "Exercises",
                newName: "TrainerId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_WorkoutPlanId",
                table: "Exercises",
                newName: "IX_Exercises_TrainerId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Exercises",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_WorkoutPlans_TrainerId_Id",
                table: "WorkoutPlans",
                columns: new[] { "TrainerId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Exercises_TrainerId_Id",
                table: "Exercises",
                columns: new[] { "TrainerId", "Id" });

            migrationBuilder.CreateTable(
                name: "WorkoutPlanExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlanExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutPlanExercises_Exercises_TrainerId_ExerciseId",
                        columns: x => new { x.TrainerId, x.ExerciseId },
                        principalTable: "Exercises",
                        principalColumns: new[] { "TrainerId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutPlanExercises_WorkoutPlans_TrainerId_WorkoutPlanId",
                        columns: x => new { x.TrainerId, x.WorkoutPlanId },
                        principalTable: "WorkoutPlans",
                        principalColumns: new[] { "TrainerId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanExercises_ExerciseId",
                table: "WorkoutPlanExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanExercises_TrainerId_ExerciseId",
                table: "WorkoutPlanExercises",
                columns: new[] { "TrainerId", "ExerciseId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanExercises_TrainerId_WorkoutPlanId",
                table: "WorkoutPlanExercises",
                columns: new[] { "TrainerId", "WorkoutPlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanExercises_WorkoutPlanId",
                table: "WorkoutPlanExercises",
                column: "WorkoutPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Trainers_TrainerId",
                table: "Exercises",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Trainers_TrainerId",
                table: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutPlanExercises");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_WorkoutPlans_TrainerId_Id",
                table: "WorkoutPlans");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Exercises_TrainerId_Id",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "TrainerId",
                table: "Exercises",
                newName: "WorkoutPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_TrainerId",
                table: "Exercises",
                newName: "IX_Exercises_WorkoutPlanId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Exercises",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Reps",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sets",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "Exercises",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_TrainerId",
                table: "WorkoutPlans",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_WorkoutPlans_WorkoutPlanId",
                table: "Exercises",
                column: "WorkoutPlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
