using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(4)]
    public class M004_RemoveOldSyncTables : Migration
    {
        public override void Up()
        {
            Delete.Table("interviewresponsibles");
            Delete.Table("interviewsyncpackagemetas");
        }

        public override void Down()
        {
            Create.Table("interviewresponsibles")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("userid").AsGuid().Nullable();

            Create.Table("interviewsyncpackagemetas")
                .WithColumn("packageid").AsString(255).PrimaryKey()
                .WithColumn("sortindex").AsInt64().Nullable()
                .WithColumn("userid").AsGuid().Nullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("itemtype").AsString().Nullable()
                .WithColumn("serializedpackagesize").AsInt32().Nullable();
        }
    }
}