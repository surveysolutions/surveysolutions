using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(10)]
    public class M010_Users_indexes : Migration
    {
        public override void Up()
        {
            this.Create.Index("users_username_indx")
                .OnTable("users")
                .OnColumn("username").Unique();

            this.Create.Index("users_username_password_indx")
                .OnTable("users")
                .OnColumn("username").Ascending()
                .OnColumn("password");
        }

        public override void Down()
        {
            this.Delete.Index("users_username_indx");
            this.Delete.Index("users_username_password_indx");
        }
    }
}