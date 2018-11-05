using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201805211530)]
    public class M201805211530_Add_Default_Responsible_to_ImportAssignments : Migration
    {
        public override void Up()
        {
            Alter.Table("assignmentsimportprocess").AddColumn("assignedto").AsGuid();
        }

        public override void Down()
        {
            Delete.Column("assignedto").FromTable("assignmentsimportprocess");
        }
    }
}
