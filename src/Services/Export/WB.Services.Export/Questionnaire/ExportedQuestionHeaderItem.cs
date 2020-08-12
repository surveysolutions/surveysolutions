using System;
using System.Collections.Generic;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Questionnaire
{
    public class ExportedQuestionHeaderItem : IExportedHeaderItem
    {
        public Guid PublicKey { get; set; }
        public QuestionType QuestionType { get; set; }
        public QuestionSubtype? QuestionSubType { get; set; }
        public string VariableName { get; set; } = String.Empty;
        public List<HeaderColumn> ColumnHeaders { get; set; } = new List<HeaderColumn>();
        public int? LengthOfRosterVectorWhichNeedToBeExported { get; set; }

        // convert to a list
        public List<LabelItem> Labels { get; set; } = new List<LabelItem>();
        public int[] ColumnValues { get; set; } = new int[0];
        public bool IsIdentifyingQuestion { get; internal set; }
        public Guid? LabelReferenceId { get; set; }
    }
}
