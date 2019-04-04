using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class ExtendToInterviewSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "key",
                table: "interview__references",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "interview__references",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_date_utc",
                table: "interview__references",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "key",
                table: "interview__references");

            migrationBuilder.DropColumn(
                name: "status",
                table: "interview__references");

            migrationBuilder.DropColumn(
                name: "update_date_utc",
                table: "interview__references");
        }
    }
}
