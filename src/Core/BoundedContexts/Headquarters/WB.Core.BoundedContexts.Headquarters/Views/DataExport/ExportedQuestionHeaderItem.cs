using System;
using System.Collections.Generic;
using System.Diagnostics;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    [DebuggerDisplay("{VariableName}")]
    [Obsolete("KP-11815")]
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

    [Obsolete("KP-11815")]
    public class LabelItem
    {
        public LabelItem() {}

        public LabelItem(Answer answer)
        {
            this.Caption = answer.AnswerValue ?? answer.AnswerText;
            this.Title = answer.AnswerText;
        }

        public string Caption { get; set; }
        public string Title { get; set; }
    }
}
