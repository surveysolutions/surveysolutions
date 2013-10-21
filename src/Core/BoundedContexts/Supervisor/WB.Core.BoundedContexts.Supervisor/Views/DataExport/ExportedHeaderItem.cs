using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class ExportedHeaderItem
    {
        public ExportedHeaderItem() {}

        public ExportedHeaderItem(IQuestion question)
        {
            PublicKey = question.PublicKey;
            QuestionType = question.QuestionType;
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
            ThrowIfQuestionIsNotMultiSelect(question);
            
            ColumnNames = new string[columnCount];
            Titles=new string[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                ColumnNames[i] = question.StataExportCaption + GetIntAsWord(i);

                if (!IsQuestionLinked(question))
                    Titles[i] += string.Format(":{0}", question.Answers[i].AnswerText);
            }
        }

        private static bool IsQuestionLinked(IQuestion question)
        {
            return question.LinkedToQuestionId.HasValue;
        }

        private void ThrowIfQuestionIsNotMultiSelect(IQuestion question)
        {
            if (question.QuestionType != QuestionType.MultyOption)
                throw new InvalidOperationException(string.Format(
                    "question '{1}' with type '{0}' can't be exported as more then one column",
                    question.QuestionType, question.QuestionText));
        }

        public Guid PublicKey { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public string[] ColumnNames { get; private set; }
        public string[] Titles { get; private set; }
       
        public Dictionary<Guid, LabelItem> Labels { get; private set; }

        protected string GetIntAsWord(int num)
        {
            var word = "";
            decimal t = num + 1;
            while (t > 0)
            {
                t--;
                word = alpha[((int) t%26)].ToString(CultureInfo.InvariantCulture) + word;
                t = Math.Floor(t/26);
            }
            return word;
        }

        private char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    }


    public class LabelItem
    {
        public LabelItem(IAnswer answer)
        {
            PublicKey = answer.PublicKey;
            Caption = answer.AnswerValue ?? answer.AnswerText;
            Title = answer.AnswerText;
        }

        public Guid PublicKey { get; private set; }
        public string Caption { get; private set; }
        public string Title { get; private set; }
    }

}
