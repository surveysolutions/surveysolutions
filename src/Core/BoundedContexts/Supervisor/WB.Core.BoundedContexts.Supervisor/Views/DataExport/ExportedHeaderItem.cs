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
            Caption = string.IsNullOrEmpty(question.StataExportCaption)
                ? question.QuestionText
                : question.StataExportCaption;
            Title = question.QuestionText;
            this.Labels = new Dictionary<Guid, LabelItem>();
            foreach (IAnswer answer in question.Answers)
            {
                this.Labels.Add(answer.PublicKey, new LabelItem(answer));
            }
        }

        public ExportedHeaderItem(IQuestion question, int index)
            : this(question)
        {
            this.Caption += GetIntAsWord(index);
            if(!question.LinkedToQuestionId.HasValue)
                this.Title += string.Format(":{0}", question.Answers[index].AnswerText);
        }

        public Guid PublicKey { get; private set; }
        public string Caption { get; private set; }
        public string Title { get; private set; }
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
