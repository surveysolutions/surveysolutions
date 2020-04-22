using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202003301600)]
    public class M202003301600_DropQuestionnaireEntitiiesAnswersTempTable : Migration
    {
        public override void Up()
        {
            Delete.Table("_temp_questionnaire_entities_answers").InSchema("readside");
        }

        public override void Down()
        {
        }
    }
}
