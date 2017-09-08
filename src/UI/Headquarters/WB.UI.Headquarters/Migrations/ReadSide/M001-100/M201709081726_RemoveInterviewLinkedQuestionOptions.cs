using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201709081726)]
    public class M201709081726_RemoveInterviewLinkedQuestionOptions : Migration
    {
        public override void Up()
        {
            Delete.Table("interviewlinkedquestionoptions");
        }

        public override void Down()
        {
        }
    }
}