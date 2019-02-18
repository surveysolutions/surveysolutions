using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class TrackDeletedQuestionnaires : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at_utc",
                table: "interview__references",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "__deleted_questionnaire_reference",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deleted_questionnaires", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__deleted_questionnaire_reference");

            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "interview__references");
        }
    }
}
