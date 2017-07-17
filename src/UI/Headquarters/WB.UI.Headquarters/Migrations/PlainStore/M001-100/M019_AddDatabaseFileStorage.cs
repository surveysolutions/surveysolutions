using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(19), Localizable(false)]
    public class M019_AddDatabaseFileStorage : Migration
    {
        public override void Up()
        {
            this.Create.Table("databasefiles")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("interviewid").AsGuid().NotNullable()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("data").AsCustom("integer[]").NotNullable();

            this.Create.Index("databasefiles_interviewid").OnTable("databasefiles");
        }

        public override void Down()
        {
            this.Delete.Index("databasefiles_interviewid").OnTable("databasefiles");
            this.Delete.Table("databasefiles");
        }
    }
}