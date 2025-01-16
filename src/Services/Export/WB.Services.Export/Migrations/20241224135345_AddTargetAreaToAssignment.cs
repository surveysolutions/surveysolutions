using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class AddTargetAreaToAssignment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "target_area",
                table: "__assignment",
                nullable: true);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "target_area",
                table: "__assignment");
        }
    }
}
