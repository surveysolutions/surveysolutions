using System;
using System.Data;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2022_04_11_18_09)]
    public class M202204111809_AddAnonymousQuestionnaireTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Create.UniqueConstraint("questionnairelistviewitems_publicid_unique")
                .OnTable("questionnairelistviewitems")
                .Column("publicid");
            
            this.Create.Table("anonymous_questionnaires")
                .WithColumn("anonymous_questionnaire_id").AsGuid().PrimaryKey()
                .WithColumn("questionnaire_id").AsGuid().NotNullable()
                    .ForeignKey("fk_questionnairelistviewitems_anonymous_questionnaires", "questionnairelistviewitems", "publicid")
                    .OnDelete(Rule.Cascade)
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("generated_at_utc").AsDateTime().NotNullable();
        }
    }
}