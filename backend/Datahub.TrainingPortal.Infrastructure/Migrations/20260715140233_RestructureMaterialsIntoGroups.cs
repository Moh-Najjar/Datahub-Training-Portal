using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.TrainingPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestructureMaterialsIntoGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialFileItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialFileItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialFileItems_TrainingMaterials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "TrainingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialLinkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialLinkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialLinkItems_TrainingMaterials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "TrainingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialFileItems_MaterialId",
                table: "MaterialFileItems",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialLinkItems_MaterialId",
                table: "MaterialLinkItems",
                column: "MaterialId");

            // Preserve existing File/Link materials by converting each into a one-item group.
            // BlobPath reproduces the OLD "{trainingId}/{storedFileName}" storage convention so
            // already-uploaded files remain downloadable (new uploads use "{trainingId}/{materialId}/{storedFileName}").
            migrationBuilder.Sql(
                """
                INSERT INTO [MaterialFileItems] ([Id],[MaterialId],[Title],[FileName],[StoredFileName],[BlobPath],[ContentType],[FileSizeBytes],[SortOrder],[CreatedAt],[IsDeleted])
                SELECT NEWID(), [Id], NULL, [FileName], [StoredFileName],
                       CAST([TrainingId] AS nvarchar(36)) + N'/' + [StoredFileName],
                       [ContentType], [FileSizeBytes], 0, GETUTCDATE(), 0
                FROM [TrainingMaterials]
                WHERE [Type] = N'File' AND [StoredFileName] IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [MaterialLinkItems] ([Id],[MaterialId],[Title],[Url],[SortOrder],[CreatedAt],[IsDeleted])
                SELECT NEWID(), [Id], NULL, [Url], 0, GETUTCDATE(), 0
                FROM [TrainingMaterials]
                WHERE [Type] = N'Link' AND [Url] IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "TrainingMaterials");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TrainingMaterials");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "TrainingMaterials");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "TrainingMaterials");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "TrainingMaterials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialFileItems");

            migrationBuilder.DropTable(
                name: "MaterialLinkItems");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "TrainingMaterials",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TrainingMaterials",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "TrainingMaterials",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "TrainingMaterials",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "TrainingMaterials",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
