using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WB.Services.Export.Migrations
{
    public partial class AddedPositionToActions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK___assignment__action",
                table: "__assignment__action");

            migrationBuilder.DropColumn(
                name: "sequence",
                table: "__assignment__action");

            migrationBuilder.AddColumn<long>(
                name: "global_sequence",
                table: "__assignment__action",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "position",
                table: "__assignment__action",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK___assignment__action",
                table: "__assignment__action",
                columns: new[] { "global_sequence", "position" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK___assignment__action",
                table: "__assignment__action");

            migrationBuilder.DropColumn(
                name: "global_sequence",
                table: "__assignment__action");

            migrationBuilder.DropColumn(
                name: "position",
                table: "__assignment__action");

            migrationBuilder.AddColumn<long>(
                name: "sequence",
                table: "__assignment__action",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK___assignment__action",
                table: "__assignment__action",
                column: "sequence");
        }
    }
}
