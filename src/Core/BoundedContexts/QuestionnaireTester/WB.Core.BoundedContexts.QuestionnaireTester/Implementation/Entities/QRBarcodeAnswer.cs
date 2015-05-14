using System;

using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class QRBarcodeAnswer : BaseInterviewAnswer
    {
        public string Answer { get; private set; }

        public QRBarcodeAnswer() { }
        public QRBarcodeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            this.Answer = answer;

            if (this.Answer.IsNullOrEmpty())
                this.MarkUnAnswered();
            else
                this.MarkAnswered();
        }
    }
}