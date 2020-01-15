using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class AddValuesColumnsToAssignmentActionExport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sequence_index",
                table: "__assignment__action",
                newName: "sequence");

            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "__assignment__action",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "new_value",
                table: "__assignment__action",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "old_value",
                table: "__assignment__action",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "audio_recording",
                table: "__assignment",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "__assignment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "__assignment",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "web_mode",
                table: "__assignment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "comment",
                table: "__assignment__action");

            migrationBuilder.DropColumn(
                name: "new_value",
                table: "__assignment__action");

            migrationBuilder.DropColumn(
                name: "old_value",
                table: "__assignment__action");

            migrationBuilder.DropColumn(
                name: "audio_recording",
                table: "__assignment");

            migrationBuilder.DropColumn(
                name: "comment",
                table: "__assignment");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "__assignment");

            migrationBuilder.DropColumn(
                name: "web_mode",
                table: "__assignment");

            migrationBuilder.RenameColumn(
                name: "sequence",
                table: "__assignment__action",
                newName: "sequence_index");
        }
    }
}
