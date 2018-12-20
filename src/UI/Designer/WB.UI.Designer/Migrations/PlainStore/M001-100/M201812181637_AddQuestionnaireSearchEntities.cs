using System;
using System.Collections.Generic;
using FluentMigrator;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201812181637)]
    public class M201812181637_AddQuestionnaireSearchEntities : Migration
    {
        private string tableName = QuestionnaireSearchStorage.TableName;
        private string tableNameWithShema = QuestionnaireSearchStorage.TableNameWithSchema;

        public override void Up()
        {
            this.Create.Table(tableName)
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("title").AsString()
                .WithColumn("entityid").AsGuid()
                .WithColumn("entity").AsCustom("jsonb")
                .WithColumn("searchtext").AsCustom("tsvector");


            this.Create.PrimaryKey($"primarykey_{tableName}")
                .OnTable(tableName)
                .Columns("questionnaireid", "entityid");


//           this.Create.Index($"searchtext_{tableName}_idx")
//               .OnTable(tableName)
//               .OnColumn("searchtext");

            this.Execute.Sql($"CREATE INDEX searchtext_{tableName}_idx ON {tableNameWithShema} USING GIN (searchtext);");

            FillTableWithData();
        }

        private void FillTableWithData()
        {
            var questionnaireSearchStorage = ServiceLocator.Current.GetInstance<IQuestionnaireSearchStorage>();
            var questionnaireSearchStorage2 = ServiceLocator.Current.GetInstance<IQuestionnaireSearchStorage>();

            Execute.WithConnection((con, trans) =>
            {
                var questionnaireIds = new List<Guid>();

                var dbCommand = con.CreateCommand();
                dbCommand.CommandText = "SELECT publicid FROM plainstore.questionnairelistviewitems WHERE ispublic = true AND isdeleted = false; ";
                using (var reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questionnaireIds.Add((Guid)reader[0]);
                    }
                }

                
            });
        }

        public override void Down()
        {
            this.Delete.Table(tableName);
        }
    }
}
