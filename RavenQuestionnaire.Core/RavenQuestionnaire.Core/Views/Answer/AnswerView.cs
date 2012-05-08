using System;
using System.Globalization;
using RavenQuestionnaire.Core.Views.Card;
using RavenQuestionnaire.Core.Entities.SubEntities;


namespace RavenQuestionnaire.Core.Views.Answer
{
    public class AnswerView
    {
        public int Index { get; set; }
        public Guid PublicKey { get; set; }
        public string AnswerValue { get; set; }
        public string AnswerText { get; set; }
        public string AnswerImage { get; set; }
        public CardView Image { get; set; }
        public bool Mandatory { get; set; }
        public AnswerType AnswerType { get; set; }
        public Guid QuestionId { get; set; }
        public string NameCollection { get; set; }

        public AnswerView()
        {
        }

        public AnswerView(Guid questionPublicKey, IAnswer doc)
        {
            this.PublicKey = doc.PublicKey;
            this.AnswerText = doc.AnswerText;
            this.AnswerValue = GetAnswerValue(doc.AnswerValue);
            this.Mandatory = doc.Mandatory;
            this.AnswerType = doc.AnswerType;
            this.QuestionId = questionPublicKey;
            this.NameCollection = doc.NameCollection;
            if (doc.Image != null)
                Image = new CardView(PublicKey, doc.Image);
        }

        public AnswerView(Guid questionPublicKey)
        {
            this.QuestionId = questionPublicKey;
        }

        protected string GetAnswerValue(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return string.Empty;

         /*   try
            {
                return DateTime.Parse(value.ToString()).ToString(@"MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);
            }
            catch (FormatException)
            {*/

                return value.ToString();
           // }

        }
    }
}
