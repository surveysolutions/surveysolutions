using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class TrackAllQuestionnairesNotOnlyDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "__deleted_questionnaire_reference", 
                newName: "__generated_questionnaire_reference");

            migrationBuilder.AddColumn<string>(
                name: "deleted_at",
                table: "__generated_questionnaire_reference",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__generated_questionnaire_reference");

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
    }
}
