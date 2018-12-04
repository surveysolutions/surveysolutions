using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201804061653)]
    public class M201804061653_ImportAssignments : Migration
    {
        public override void Up()
        {
            Create.Table("assignmenttoimport")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("interviewer").AsGuid().Nullable()
                .WithColumn("supervisor").AsGuid().Nullable()
                .WithColumn("quantity").AsInt32().Nullable()
                .WithColumn("answers").AsCustom("jsonb").Nullable()
                .WithColumn("verified").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("error").AsString().Nullable();

            Create.Table("assignmentsimportprocess")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("questionnaireid").AsString().NotNullable()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("totalcount").AsInt32().NotNullable()
                .WithColumn("responsible").AsString().NotNullable()
                .WithColumn("starteddate").AsDateTime().NotNullable();
        }

        public override void Down()
        {
        }
    }
}
