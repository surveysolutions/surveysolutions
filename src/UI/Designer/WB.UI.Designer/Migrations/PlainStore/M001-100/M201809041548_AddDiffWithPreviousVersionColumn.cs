using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201809041548)]
    [Localizable(false)]
    public class M201809041548_AddDiffWithPreviousVersionColumn : Migration
    {
        public override void Up()
        {
            Create.Column("diffwithpreviousversion").OnTable("questionnairechangerecords").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("diffwithpreviousversion").FromTable("questionnairechangerecords");
        }
    }
}
