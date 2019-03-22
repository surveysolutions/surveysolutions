using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201902131321)]
    [Localizable(false)]
    public class M201902131321_AddEmailAndPasswordToAssignmentAndImport : Migration
    {
        public override void Up()
        {
            Create.Column("email").OnTable("assignments").AsString().Nullable();
            Create.Column("password").OnTable("assignments").AsString().Nullable();

            Create.Column("email").OnTable("assignmenttoimport").AsString().Nullable();
            Create.Column("password").OnTable("assignmenttoimport").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("email").FromTable("assignments");
            Delete.Column("password").FromTable("assignments");

            Delete.Column("email").FromTable("assignmenttoimport");
            Delete.Column("password").FromTable("assignmenttoimport");
        }
    }
}
