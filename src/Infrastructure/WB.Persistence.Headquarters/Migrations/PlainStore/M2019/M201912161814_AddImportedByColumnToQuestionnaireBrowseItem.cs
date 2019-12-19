using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201912161814)]
    public class M201912161814_AddImportedByColumnToQuestionnaireBrowseItem : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("importedby").OnTable("questionnairebrowseitems").AsGuid().Nullable();
        }
    }
}
