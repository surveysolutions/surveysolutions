using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class ExportedHeaderItem
    {
        public Guid PublicKey { get; set; }
        public QuestionType QuestionType { get; set; }
        public int? DepthOfLinkedQuestionRelativeToSource { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] Titles { get; set; }
        public string VariableName { get; set; }
        public Dictionary<Guid, LabelItem> Labels { get; set; }}


    public class LabelItem
    {
        public LabelItem() {}

        public LabelItem(IAnswer answer)
        {
            PublicKey = answer.PublicKey;
            Caption = answer.AnswerValue ?? answer.AnswerText;
            Title = answer.AnswerText;
        }

        public Guid PublicKey { get; set; }
        public string Caption { get; set; }
        public string Title { get; set; }
    }

}
