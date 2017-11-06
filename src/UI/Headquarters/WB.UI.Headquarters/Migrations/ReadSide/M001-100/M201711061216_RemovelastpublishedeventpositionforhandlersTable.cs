using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Localizable(false)]
    [Migration(201711061216)]
    public class M201711061216_RemovelastpublishedeventpositionforhandlersTable : Migration
    {
        public override void Up()
        {
            Delete.Table("lastpublishedeventpositionforhandlers");
        }

        public override void Down()
        {
        }
    }
}