using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201907161538)]
    [Localizable(false)]
    public class M201907161538_AddHasResolvedCommentsColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("hasresolvedcomments")
                .OnTable("interviewsummaries")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);
        }
    }
}
