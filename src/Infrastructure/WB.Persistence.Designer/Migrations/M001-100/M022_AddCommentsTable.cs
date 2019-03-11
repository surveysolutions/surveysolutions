using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(22)]
    public class M022_AddCommentsTable : Migration
    {
        private string commentsTableName = "commentinstances";

        public override void Up()
        {
            Create.Table(commentsTableName)
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().NotNullable()
                .WithColumn("entityid").AsGuid().NotNullable()
                .WithColumn("date").AsDateTime().NotNullable()
                .WithColumn("resolvedate").AsDateTime().Nullable()
                .WithColumn("comment").AsString().NotNullable()
                .WithColumn("useremail").AsString().NotNullable()
                .WithColumn("username").AsString().NotNullable();

            Create.Index($"{commentsTableName}_questionnaire")
                .OnTable(commentsTableName)
                .OnColumn("questionnaireid").Ascending()
                .OnColumn("entityid").Ascending();
        }

        public override void Down()
        {
            Delete.Table(commentsTableName);
        }
    }
}
