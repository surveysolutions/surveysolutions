using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(7)]
    public class M007_AddAssemblyInfos : Migration
    {
        public override void Up()
        {
            Create.Table("assemblyinfos")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("creationdate").AsDateTime().Nullable()
                .WithColumn("content").AsBinary().Nullable();
        }

        public override void Down()
        {
            Delete.Table("assemblyinfos");
        }
    }
}