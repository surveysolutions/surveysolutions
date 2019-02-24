using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class ConvertMetadataToKV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "value",
                table: "metadata",
                nullable: true);

            migrationBuilder.Sql(
                "insert into metadata (id, value, global_sequence) " +
                "select 'globalSequence' as id, global_sequence::text as value, global_sequence from metadata");
            
            migrationBuilder.DropColumn(
                name: "global_sequence",
                table: "metadata");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "value",
                table: "metadata");

            migrationBuilder.AddColumn<long>(
                name: "global_sequence",
                table: "metadata",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
