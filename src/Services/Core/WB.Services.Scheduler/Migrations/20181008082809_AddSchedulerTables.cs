using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WB.Services.Scheduler.Migrations
{
    public partial class AddSchedulerTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduler");

            migrationBuilder.CreateTable(
                name: "jobs",
                schema: "scheduler",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    type = table.Column<string>(nullable: false),
                    args = table.Column<string>(nullable: false),
                    tag = table.Column<string>(nullable: true),
                    tenant = table.Column<string>(nullable: false),
                    status = table.Column<string>(nullable: false),
                    start_at = table.Column<DateTime>(nullable: true),
                    end_at = table.Column<DateTime>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    last_update_at = table.Column<DateTime>(nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    schedule_at = table.Column<DateTime>(nullable: true),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jobs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_tenant_status",
                schema: "scheduler",
                table: "jobs",
                columns: new[] { "tenant", "status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jobs",
                schema: "scheduler");
        }
    }
}
