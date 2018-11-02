using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(12)]
    public class M012_AddAllowAssignmentsColumnToQuestionnaireBrowseItem : Migration
    {
        private const string questionnaireBrowseItemsTable = "questionnairebrowseitems";
        private const string allowAssignmentsColumn = "allowassignments";

        public override void Up()
        {
            this.Create.Column(allowAssignmentsColumn)
                .OnTable(questionnaireBrowseItemsTable)
                .AsBoolean()
                .NotNullable()
                .SetExistingRowsTo(false);
        }

        public override void Down()
        {
            Delete.Column(allowAssignmentsColumn)
                .FromTable(questionnaireBrowseItemsTable);
        }
    }
}