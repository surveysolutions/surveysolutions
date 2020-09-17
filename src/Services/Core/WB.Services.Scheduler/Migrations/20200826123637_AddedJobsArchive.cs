using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WB.Services.Scheduler.Migrations
{
    public partial class AddedJobsArchive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tenant_name",
                schema: "scheduler",
                table: "jobs",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Data",
                schema: "scheduler",
                table: "jobs",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "scheduler",
                table: "jobs",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.CreateTable(
                name: "archive",
                schema: "scheduler",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(nullable: false),
                    args = table.Column<string>(nullable: false),
                    tenant = table.Column<string>(nullable: true),
                    tenant_name = table.Column<string>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    start_at = table.Column<DateTime>(nullable: true),
                    end_at = table.Column<DateTime>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    last_update_at = table.Column<DateTime>(nullable: false),
                    schedule_at = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_archive", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "archive",
                schema: "scheduler");

            migrationBuilder.AlterColumn<string>(
                name: "tenant_name",
                schema: "scheduler",
                table: "jobs",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Data",
                schema: "scheduler",
                table: "jobs",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "scheduler",
                table: "jobs",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
