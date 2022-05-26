using System;
using System.Data;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2022_04_11_18_08)]
    public class M202204111808_AddUniqueConstraintToPublicId : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Create.UniqueConstraint("questionnairelistviewitems_publicid_unique")
                .OnTable("questionnairelistviewitems")
                .Column("publicid");
        }
    }
}