using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201802151238)]
    [Localizable(false)]
    public class M201802151238_UpdateFunctionToUpdateInterview : Migration
    {
        public override void Up()
        {
            Alter.Table("interviews").AddColumn("warnings").AsCustom("integer[]").Nullable();

            Execute.EmbeddedScript("M201802151238_interview_update.sql");
        }

        public override void Down()
        {
        }
    }
}