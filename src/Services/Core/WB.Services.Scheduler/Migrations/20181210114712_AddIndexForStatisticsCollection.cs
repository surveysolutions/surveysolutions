using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Scheduler.Migrations
{
    public partial class AddIndexForStatisticsCollection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_jobs_type_status_tenant",
                schema: "scheduler",
                table: "jobs",
                columns: new[] { "type", "status", "tenant" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_jobs_type_status_tenant",
                schema: "scheduler",
                table: "jobs");
        }
    }
}
