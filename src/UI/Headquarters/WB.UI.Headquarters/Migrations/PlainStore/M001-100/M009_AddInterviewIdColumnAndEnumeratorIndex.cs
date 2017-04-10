using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(9)]
    public class M009_AddInterviewIdColumnAndEnumeratorIndex : Migration
    {
        private readonly string synchronizationLogTable = "synchronizationlog";
        private readonly string interviewIdColumn = "interviewid";
        private string enumeratorColumnName = "interviewerid";
        private const string enumeratoridIndexName = "synchronizationlog_interviewerid";
        private const string interviewidIndexName = "synchronizationlog_interviewid";
        public override void Up()
        {
            Alter.Table(this.synchronizationLogTable).AddColumn(this.interviewIdColumn).AsGuid().Nullable();

            if (!Schema.Table(this.synchronizationLogTable).Index(enumeratoridIndexName).Exists())
            {
                Create.Index(enumeratoridIndexName)
                    .OnTable(this.synchronizationLogTable)
                    .OnColumn(this.enumeratorColumnName);
            }

            if (!Schema.Table(this.synchronizationLogTable).Index(interviewidIndexName).Exists())
            {
                Create.Index(interviewidIndexName)
                    .OnTable(this.synchronizationLogTable)
                    .OnColumn(this.interviewIdColumn);
            }
        }

        public override void Down()
        {
            Delete.Column(this.interviewIdColumn).FromTable(synchronizationLogTable);
            Delete.Index(enumeratoridIndexName);
            Delete.Index(interviewidIndexName);
        }
    }
}