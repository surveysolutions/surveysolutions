using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201801051838)]
    [Localizable(false)]
    public class M201801051838_SpeedupUploadInterview : Migration
    {
        public override void Up()
        {
            Delete.Column("entitytype").FromTable("interviews");
            Delete.Column("answertype").FromTable("interviews");
            
            Execute.EmbeddedScript("interview_update.sql");
        }

        public override void Down()
        {
        }
    }
}