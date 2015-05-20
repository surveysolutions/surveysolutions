using System;

using WB.Core.GenericSubdomains.Portable;

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
            this.IsAnswered = !this.Answer.IsNullOrEmpty();
        }

        public override void RemoveAnswer()
        {
            this.IsAnswered = false;
            this.Answer = null;
        }
    }
}