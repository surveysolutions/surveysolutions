using System;

namespace Main.Core.Entities.SubEntities
{
    public class Answer
    {
        public Answer()
        {
            this.PublicKey = Guid.NewGuid();
        }

        public string AnswerText { get; set; }

        public string AnswerValue { get; set; }
       
        public Guid PublicKey { get; set; }

        public Answer Clone()
        {
            return this.MemberwiseClone() as Answer;
        }

        public static Answer CreateFromOther(Answer answer)
        {
            return new Answer
            {
                AnswerText = answer.AnswerText,
                AnswerValue = answer.AnswerValue,
                PublicKey = answer.PublicKey,
            };
        }
    }
}