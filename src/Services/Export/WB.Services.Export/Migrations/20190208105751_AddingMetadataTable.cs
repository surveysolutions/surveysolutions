using Microsoft.EntityFrameworkCore.Migrations;

namespace WB.Services.Export.Migrations
{
    public partial class AddingMetadataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metadata",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    global_sequence = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_metadata", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metadata");
        }
    }
}
