using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201910091400, "Added meta field to change records", BreakingChange = false)]
    public class M201910091400_AddedMetaFieldToChangeRecords : AutoReversingMigration
    {
        public override void Up()
        {
            Alter.Table("questionnairechangerecords")
                .AddColumn("meta").AsCustom("jsonb").Nullable();                
        }
    }
}
