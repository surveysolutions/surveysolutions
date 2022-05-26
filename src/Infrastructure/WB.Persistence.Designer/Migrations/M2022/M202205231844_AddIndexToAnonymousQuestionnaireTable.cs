using System;
using System.Data;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2022_05_23_18_44)]
    public class M202205231844_AddIndexToAnonymousQuestionnaireTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Create.Index("anonymous_questionnaires_questionnaire_id")
                .OnTable("anonymous_questionnaires")
                .OnColumn("questionnaire_id");
        }
    }
}