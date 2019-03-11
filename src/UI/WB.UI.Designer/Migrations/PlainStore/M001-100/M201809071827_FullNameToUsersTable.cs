using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201809071827)]
    public class M201809071827_FullNameToUsersTable : Migration
    {
        public override void Up()
        {
            this.Alter.Table("users").AddColumn("fullname").AsString().Nullable();
        }

        public override void Down()
        {
            this.Delete.Column("fullname").FromTable("users");
        }
    }
}
