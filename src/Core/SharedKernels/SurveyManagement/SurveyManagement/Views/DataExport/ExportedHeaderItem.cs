using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class ExportedHeaderItem
    {
        public Guid PublicKey { get; set; }
        public QuestionType QuestionType { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] Titles { get; set; }
        public string VariableName { get; set; }
        public int? LengthOfRosterVectorWhichNeedToBeExported { get; set; }
        public Dictionary<Guid, LabelItem> Labels { get; set; }
    }


    public class LabelItem
    {
        public LabelItem() {}

        public LabelItem(Answer answer)
        {
            this.PublicKey = answer.PublicKey;
            this.Caption = answer.AnswerValue ?? answer.AnswerText;
            this.Title = answer.AnswerText;
        }

        public Guid PublicKey { get; set; }
        public string Caption { get; set; }
        public string Title { get; set; }
    }

}
