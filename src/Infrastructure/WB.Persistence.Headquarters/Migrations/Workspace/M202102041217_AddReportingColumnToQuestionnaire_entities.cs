using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202102041217)]
    public class M202102041217_AddReportingColumnToQuestionnaire_entities : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Column("included_in_reporting_at_utc")
                .OnTable("questionnaire_entities")
                .AsDateTime()
                .Nullable();

            Create.Column("variable_type")
                .OnTable("questionnaire_entities")
                .AsInt32()
                .Nullable();
        }
    }
}
