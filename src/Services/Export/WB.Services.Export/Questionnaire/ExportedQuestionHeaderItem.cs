using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Questionnaire
{
    public class ExportedQuestionHeaderItem : IExportedHeaderItem
    {
        public Guid PublicKey { get; set; }
        public QuestionType QuestionType { get; set; }
        public QuestionSubtype? QuestionSubType { get; set; }
        public string VariableName { get; set; }
        public List<HeaderColumn> ColumnHeaders { get; set; }
        public int? LengthOfRosterVectorWhichNeedToBeExported { get; set; }

        // convert to a list
        public List<LabelItem> Labels { get; set; }
        public int[] ColumnValues { get; set; }
        public bool IsIdentifyingQuestion { get; internal set; }
    }
}