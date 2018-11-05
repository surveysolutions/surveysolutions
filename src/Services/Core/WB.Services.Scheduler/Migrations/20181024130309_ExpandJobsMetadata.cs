using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Scheduler.Migrations
{
    public partial class ExpandJobsMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tenant_name",
                schema: "scheduler",
                table: "jobs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tenant_name",
                schema: "scheduler",
                table: "jobs");
        }
    }
}
