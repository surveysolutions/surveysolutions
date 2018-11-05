using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(24)]
    public class M024_AddAllowExportVariablesColumnToQuestionnaireBrowseItem : Migration
    {
        private const string questionnaireBrowseItemsTable = "questionnairebrowseitems";
        private const string allowExportVariables = "allowexportvariables";

        public override void Up()
        {
            this.Create.Column(allowExportVariables)
                .OnTable(questionnaireBrowseItemsTable)
                .AsBoolean()
                .NotNullable()
                .SetExistingRowsTo(false);
        }

        public override void Down()
        {
            Delete.Column(allowExportVariables)
                .FromTable(questionnaireBrowseItemsTable);
        }
    }
}