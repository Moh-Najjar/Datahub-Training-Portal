using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.TrainingPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSortOrderToTrainingMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "TrainingMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                WITH Ordered AS (
                    SELECT [Id], ROW_NUMBER() OVER (PARTITION BY [TrainingId] ORDER BY [CreatedAt]) - 1 AS [Rank]
                    FROM [TrainingMaterials]
                )
                UPDATE m
                SET m.[SortOrder] = o.[Rank]
                FROM [TrainingMaterials] m
                JOIN Ordered o ON o.[Id] = m.[Id];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "TrainingMaterials");
        }
    }
}
