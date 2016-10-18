using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(9)]
    public class M009_Users : Migration
    {
        public override void Up()
        {
            this.Create.Table("users")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("applicationname").AsString().Nullable()
                .WithColumn("comment").AsString().Nullable()
                .WithColumn("confirmationtoken").AsString().Nullable()
                .WithColumn("createdat").AsDateTime().Nullable()
                .WithColumn("email").AsString().Nullable()
                .WithColumn("isconfirmed").AsBoolean().Nullable()
                .WithColumn("islockedout").AsBoolean().Nullable()
                .WithColumn("isonline").AsBoolean().Nullable()
                .WithColumn("lastactivityat").AsDateTime().Nullable()
                .WithColumn("lastlockedoutat").AsDateTime().Nullable()
                .WithColumn("lastloginat").AsDateTime().Nullable()
                .WithColumn("lastpasswordchangeat").AsDateTime().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("passwordanswer").AsString().Nullable()
                .WithColumn("passwordquestion").AsString().Nullable()
                .WithColumn("passwordresetexpirationdate").AsDateTime().Nullable()
                .WithColumn("passwordresettoken").AsString().Nullable()
                .WithColumn("passwordsalt").AsString().Nullable()
                .WithColumn("provideruserkey").AsGuid().Nullable()
                .WithColumn("username").AsString().Nullable();

            this.Create.Table("simpleroles")
                .WithColumn("userid").AsString(255)
                .WithColumn("simpleroleid").AsInt32().Nullable();

            this.Create.ForeignKey()
                .FromTable("simpleroles").ForeignColumn("userid")
                .ToTable("users").PrimaryColumn("id");

            this.Create.Index("simpleroles_userid_indx")
                .OnTable("simpleroles")
                .OnColumn("userid").Ascending();
        }

        public override void Down()
        {
            this.Delete.Table("users");
            this.Delete.Table("simpleroles");
            this.Delete.Index("simpleroles_userid_indx");
        }
    }
}