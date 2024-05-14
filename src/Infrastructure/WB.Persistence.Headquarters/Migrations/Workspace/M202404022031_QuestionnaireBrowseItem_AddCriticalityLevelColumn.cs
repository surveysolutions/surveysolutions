using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202404022031)]
    public class M202404022031_QuestionnaireBrowseItem_Add_Criticality_Level_Column : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("criticality_level")
                .OnTable("questionnairebrowseitems")
                .AsInt16()
                .Nullable();
            
            Create.Column("criticality_support")
                .OnTable("questionnairebrowseitems")
                .AsBoolean()
                .Nullable();
        }
    }
}
