using System;
using System.Data;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2022_07_12_13_11)]
    public class M202207121311_RenameColumnCreatedByToOwnerId : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Rename.Column("createdby")
                .OnTable("questionnairelistviewitems")
                .To("ownerid");
        }
    }
}