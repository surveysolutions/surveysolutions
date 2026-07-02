using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_05_18_12_00)]
    public class M202605181200_AddIndexOnQuestionnaireChangeRecordsLatestRevision : ForwardOnlyMigration
    {
        private const string TableName = "questionnairechangerecords";
        private const string IndexName = "questionnairechangerecords_questionnaireid_sequence_desc";

        public override void Up()
        {
            if (!Schema.Table(TableName).Exists())
                return;

            if (Schema.Table(TableName).Index(IndexName).Exists())
                return;

            Create.Index(IndexName)
                .OnTable(TableName)
                .OnColumn("questionnaireid").Ascending()
                .OnColumn("sequence").Descending();
        }
    }
}
