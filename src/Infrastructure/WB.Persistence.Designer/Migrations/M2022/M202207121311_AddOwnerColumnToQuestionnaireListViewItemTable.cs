using System;
using System.Data;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2022_07_12_13_11)]
    public class M202207121311_AddOwnerColumnToQuestionnaireListViewItemTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Create.Column("ownerid")
                .OnTable("questionnairelistviewitems")
                .AsGuid()
                .Nullable()
                .Indexed();

            this.Execute.Sql("update plainstore.questionnairelistviewitems SET ownerid = createdby;");
        }
    }
}