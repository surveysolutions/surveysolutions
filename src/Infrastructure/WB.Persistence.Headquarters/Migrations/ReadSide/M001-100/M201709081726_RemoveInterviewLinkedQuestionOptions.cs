using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201709081726)]
    public class M201709081726_RemoveInterviewLinkedQuestionOptions : Migration
    {
        public override void Up()
        {
            if (Schema.Table("interviewlinkedquestionoptions").Exists())
            {
                Delete.Table("interviewlinkedquestionoptions");
            }
        }

        public override void Down()
        {
        }
    }
}