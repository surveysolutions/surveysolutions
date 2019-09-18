using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class AddAssignmentTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assignment",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    publicKey = table.Column<Guid>(nullable: false),
                    responsibleId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment__key", x => x.publicKey);
                    table.UniqueConstraint("unique_assignment__id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assignment__action",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false),
                    originatorId = table.Column<Guid>(nullable: false),
                    responsibleId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment__action", x => x.id);
                });

            migrationBuilder.AddColumn<int>(
                name: "assignmentId",
                table: "interview__references",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assignmentId",
                table: "interview__references");

            migrationBuilder.DropTable(name: "assignment__action");
            migrationBuilder.DropTable(name: "assignment");
        }
    }
}
