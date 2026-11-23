using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.TrainingPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentIdToEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EnrollmentId",
                table: "Evaluations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EnrollmentId",
                table: "Evaluations",
                column: "EnrollmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Enrollments_EnrollmentId",
                table: "Evaluations",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Enrollments_EnrollmentId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_EnrollmentId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "EnrollmentId",
                table: "Evaluations");
        }
    }
}
