using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201711141615)]
    public class M201711141615_ImportUsers : Migration
    {
        public override void Up()
        {
            Delete.Table("userpreloadingverificationerrors");

            Execute.Sql("DELETE FROM plainstore.userprelodingdata;" +
                        "DELETE FROM plainstore.userpreloadingprocesses");
            
            Delete.Column("filesize").FromTable("userpreloadingprocesses");
            Delete.Column("state").FromTable("userpreloadingprocesses");
            Delete.Column("uploaddate").FromTable("userpreloadingprocesses");
            Delete.Column("validationstartdate").FromTable("userpreloadingprocesses");
            Delete.Column("verificationprogressinpercents").FromTable("userpreloadingprocesses");
            Delete.Column("creationstartdate").FromTable("userpreloadingprocesses");
            Delete.Column("lastupdatedate").FromTable("userpreloadingprocesses");
            Delete.Column("createduserscount").FromTable("userpreloadingprocesses");
            Delete.Column("errormessage").FromTable("userpreloadingprocesses");

            Create.Column("supervisorscount").OnTable("userpreloadingprocesses").AsInt32();
            Create.Column("interviewerscount").OnTable("userpreloadingprocesses").AsInt32();
        }

        public override void Down()
        {
        }
    }
}