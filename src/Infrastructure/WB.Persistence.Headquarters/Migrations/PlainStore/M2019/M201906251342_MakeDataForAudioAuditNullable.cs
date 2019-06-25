using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201906251342)]
    public class M201906251342_MakeDataForAudioAuditNullable : Migration
    {
        public override void Up()
        {
            Alter.Column("data").OnTable("audioauditfiles").AsBinary().Nullable();
        }

        public override void Down()
        {
        }
    }
}
