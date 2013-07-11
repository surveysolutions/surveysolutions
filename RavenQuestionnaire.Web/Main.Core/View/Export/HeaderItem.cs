using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.Export
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HeaderItem
    {
        public HeaderItem()
        {
        }

        public HeaderItem(IQuestion question)
        {
            PublicKey = question.PublicKey;
            Caption = string.IsNullOrEmpty(question.StataExportCaption)
                          ? question.QuestionText
                          : question.StataExportCaption;
            Title = question.QuestionText;
            this.Labels=new Dictionary<Guid, LabelItem>();
            foreach (IAnswer answer in question.Answers)
            {
                this.Labels.Add(answer.PublicKey, new LabelItem(answer));
            }
        }
        public HeaderItem(IQuestion question,int index):this(question)
        {
            this.Caption += GetIndexLeter(index, question.Answers.Count);
            this.Title += string.Format(":{0}", question.Answers[index].AnswerText);
        }

        public Guid PublicKey { get; private set; }
        public string Caption { get; private set; }
        public string Title { get; private set; }
        public Dictionary<Guid, LabelItem> Labels { get; private set; }

        protected string GetIndexLeter(int index, int count)
        {
            if (count < alpha.Length)
                return alpha[index].ToString();
            int left = count/alpha.Length;
            int byteCount = 1;
            while (left>0)
            {
                byteCount++;

                left = count/(int) Math.Pow(alpha.Length, byteCount);
            }
            var result = new char[byteCount];
            int tempIndex = index;
            for (int i = 0; i < byteCount; i++)
            {
                int currentIndex = tempIndex/(int) Math.Pow(alpha.Length, byteCount - i - 1);
                result[i] = alpha[currentIndex];

                var tempIndexMinusByte = tempIndex - (int)Math.Pow(alpha.Length, byteCount - i - 1);
                if (tempIndexMinusByte > 0)
                    tempIndex = tempIndexMinusByte;
            }
            return new string(result);
        }
        
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
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
