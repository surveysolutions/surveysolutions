using System;
using System.Data;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2022_05_04_14_17)]
    public class M202205041417_AddColumnGeneratedAtUtcInAnonymousQuestionnaireTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Create.Column("generatedatutc").OnTable("anonymousquestionnaires")
                .AsDateTime()
                .SetExistingRowsTo(DateTime.UtcNow)
                .NotNullable();
        }
    }
}