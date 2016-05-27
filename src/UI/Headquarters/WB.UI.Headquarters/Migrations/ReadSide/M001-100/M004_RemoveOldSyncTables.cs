using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(4)]
    public class M004_RemoveOldSyncTables : Migration
    {
        public override void Up()
        {
            Delete.Table("interviewresponsibles");
        }

        public override void Down()
        {
            Create.Table("interviewresponsibles")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("userid").AsGuid().Nullable();
        }
    }
}