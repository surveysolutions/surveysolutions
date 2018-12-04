using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201709151330)]
    public class M0201709151330_DeleteInterviewEntitiesTable : Migration
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