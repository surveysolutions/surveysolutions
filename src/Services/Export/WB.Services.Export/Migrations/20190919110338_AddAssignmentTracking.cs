using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WB.Services.Export.Migrations
{
    public partial class AddAssignmentTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "assignment_id",
                table: "interview__references",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "__assignment",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    public_key = table.Column<Guid>(nullable: false),
                    responsible_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignments", x => x.id);
                    table.UniqueConstraint("AK___assignment_public_key", x => x.public_key);
                });

            migrationBuilder.CreateTable(
                name: "__assignment__action",
                columns: table => new
                {
                    sequence_index = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    assignment_id = table.Column<int>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false),
                    originator_id = table.Column<Guid>(nullable: false),
                    responsible_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___assignment__action", x => x.sequence_index);
                    table.ForeignKey(
                        name: "fk_assignment_actions_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "__assignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assignment_actions_assignment_id",
                table: "__assignment__action",
                column: "assignment_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__assignment__action");

            migrationBuilder.DropTable(
                name: "__assignment");

            migrationBuilder.DropColumn(
                name: "assignment_id",
                table: "interview__references");
        }
    }
}
