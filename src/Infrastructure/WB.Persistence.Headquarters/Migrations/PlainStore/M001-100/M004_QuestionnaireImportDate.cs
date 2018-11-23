using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(4)]
    public class M004_QuestionnaireImportDate : Migration
    {
        public override void Up()
        {
            Create.Column("importdate").OnTable("questionnairebrowseitems").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Column("importdate").FromTable("questionnairebrowseitems");
        }
    }
}