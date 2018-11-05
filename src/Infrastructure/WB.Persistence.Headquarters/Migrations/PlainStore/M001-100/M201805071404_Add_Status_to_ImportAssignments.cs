using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201805071404)]
    public class M201805071404_Add_Status_to_ImportAssignments : Migration
    {
        public override void Up()
        {
            Alter.Table("assignmentsimportprocess").AddColumn("status").AsInt32().SetExistingRowsTo(1);
        }

        public override void Down()
        {
            Delete.Column("status").FromTable("assignmentsimportprocess");
        }
    }
}
