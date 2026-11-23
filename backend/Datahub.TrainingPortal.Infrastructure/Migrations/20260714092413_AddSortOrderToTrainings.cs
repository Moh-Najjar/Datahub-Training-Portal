using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.TrainingPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSortOrderToTrainings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Trainings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                WITH Ordered AS (
                    SELECT [Id], ROW_NUMBER() OVER (PARTITION BY [DepartmentId] ORDER BY [CreatedAt]) - 1 AS [Rank]
                    FROM [Trainings]
                )
                UPDATE t
                SET t.[SortOrder] = o.[Rank]
                FROM [Trainings] t
                JOIN Ordered o ON o.[Id] = t.[Id];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Trainings");
        }
    }
}
