using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Scheduler.Migrations
{
    public partial class AddedWorkerIdThatHandleJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "worker_id",
                schema: "scheduler",
                table: "jobs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "worker_id",
                schema: "scheduler",
                table: "jobs");
        }
    }
}
