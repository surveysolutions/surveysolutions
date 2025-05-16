using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WB.Services.Scheduler.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForNet9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_at",
                schema: "scheduler",
                table: "archive",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "(now() at time zone 'utc')",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "scheduler",
                table: "archive",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "(now() at time zone 'utc')",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_at",
                schema: "scheduler",
                table: "archive",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "(now() at time zone 'utc')");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "scheduler",
                table: "archive",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "(now() at time zone 'utc')");
        }
    }
}
