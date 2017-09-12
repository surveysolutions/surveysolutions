using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(24)]
    public class M024_DeleteInterviewEntitiesTable : Migration
    {
        public override void Up()
        {
            Delete.Table("interviewentities");
        }

        public override void Down()
        {
        }
    }
}