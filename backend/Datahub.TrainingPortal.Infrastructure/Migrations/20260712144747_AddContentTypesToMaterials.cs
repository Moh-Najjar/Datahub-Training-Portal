using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.TrainingPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypesToMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StoredFileName",
                table: "TrainingMaterials",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260);

            migrationBuilder.AlterColumn<long>(
                name: "FileSizeBytes",
                table: "TrainingMaterials",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "TrainingMaterials",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "TrainingMaterials",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<string>(
                name: "TextContent",
                table: "TrainingMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TrainingMaterials",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TrainingMaterials",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "TrainingMaterials",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [TrainingMaterials] SET [Type] = 'File', [Title] = [FileName] WHERE [Type] = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextContent",
                table: "TrainingMaterials");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TrainingMaterials");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TrainingMaterials");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "TrainingMaterials");

            migrationBuilder.AlterColumn<string>(
                name: "StoredFileName",
                table: "TrainingMaterials",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FileSizeBytes",
                table: "TrainingMaterials",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "TrainingMaterials",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "TrainingMaterials",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);
        }
    }
}
