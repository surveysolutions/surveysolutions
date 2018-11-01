using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(3)]
    public class M003_RemoveFeedTables : Migration
    {
        public override void Up()
        {
            this.Delete.Table("questionnairefeedentries");
            this.Delete.Table("userchangedfeedentries");
            this.Delete.Table("interviewfeedentries");
        }

        public override void Down()
        {
            this.Create.Table("interviewfeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("supervisorid").AsString().Nullable()
                .WithColumn("entrytype").AsInt32().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable()
                .WithColumn("interviewid").AsString().Nullable()
                .WithColumn("userid").AsString().Nullable()
                .WithColumn("interviewerid").AsString().Nullable();

            this.Create.Table("questionnairefeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable()
                .WithColumn("entrytype").AsInt32().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable();

            this.Create.Table("userchangedfeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("changeduserid").AsString().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable()
                .WithColumn("supervisorid").AsString().Nullable()
                .WithColumn("entrytype").AsInt32().Nullable();
        }
    }
}