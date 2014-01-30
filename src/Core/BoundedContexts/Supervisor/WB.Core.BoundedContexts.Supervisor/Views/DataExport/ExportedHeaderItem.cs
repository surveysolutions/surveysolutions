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
        public ExportedHeaderItem() {}

        public ExportedHeaderItem(IQuestion question)
        {
            PublicKey = question.PublicKey;
            QuestionType = question.QuestionType;
            VariableName = question.StataExportCaption;
            Titles = new string[]{question.QuestionText};
            ColumnNames = new string[] { question.StataExportCaption };

            this.Labels = new Dictionary<Guid, LabelItem>();
            foreach (IAnswer answer in question.Answers)
            {
                this.Labels.Add(answer.PublicKey, new LabelItem(answer));
            }
        }

        public ExportedHeaderItem(IQuestion question, int columnCount)
            : this(question)
        {
            this.ThrowIfQuestionIsNotMultiSelectOrTextList(question);
            
            ColumnNames = new string[columnCount];
            Titles=new string[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                ColumnNames[i] = string.Format("{0}_{1}", question.StataExportCaption, i);

                if (!IsQuestionLinked(question))
                {
                    if(question is IMultyOptionsQuestion)
                        Titles[i] += string.Format(":{0}", question.Answers[i].AnswerText);
                    if(question is ITextListQuestion)
                        Titles[i] += string.Format(":{0}", i);
                }
            }
        }

        private static bool IsQuestionLinked(IQuestion question)
        {
            return question.LinkedToQuestionId.HasValue;
        }

        private void ThrowIfQuestionIsNotMultiSelectOrTextList(IQuestion question)
        {
            if (question.QuestionType != QuestionType.MultyOption && question.QuestionType != QuestionType.TextList)
                throw new InvalidOperationException(string.Format(
                    "question '{1}' with type '{0}' can't be exported as more then one column",
                    question.QuestionType, question.QuestionText));
        }

        public Guid PublicKey { get; set; }
        public QuestionType QuestionType { get; set; }
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
