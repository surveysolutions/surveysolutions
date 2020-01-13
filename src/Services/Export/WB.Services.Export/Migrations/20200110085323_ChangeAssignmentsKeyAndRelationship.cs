using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WB.Services.Export.Migrations
{
    public partial class ChangeAssignmentsKeyAndRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_assignment_actions_assignments_assignment_id",
                table: "__assignment__action");

            migrationBuilder.DropPrimaryKey(
                name: "pk_assignments",
                table: "__assignment");

            migrationBuilder.DropUniqueConstraint(
                name: "AK___assignment_public_key",
                table: "__assignment");

            migrationBuilder.RenameIndex(
                name: "ix_assignment_actions_assignment_id",
                table: "__assignment__action",
                newName: "IX___assignment__action_assignment_id");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "__assignment",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK___assignment_id",
                table: "__assignment",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK___assignment",
                table: "__assignment",
                column: "public_key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK___assignment_id",
                table: "__assignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK___assignment",
                table: "__assignment");

            migrationBuilder.RenameIndex(
                name: "IX___assignment__action_assignment_id",
                table: "__assignment__action",
                newName: "ix_assignment_actions_assignment_id");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "__assignment",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "pk_assignments",
                table: "__assignment",
                column: "id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK___assignment_public_key",
                table: "__assignment",
                column: "public_key");

            migrationBuilder.AddForeignKey(
                name: "fk_assignment_actions_assignments_assignment_id",
                table: "__assignment__action",
                column: "assignment_id",
                principalTable: "__assignment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
