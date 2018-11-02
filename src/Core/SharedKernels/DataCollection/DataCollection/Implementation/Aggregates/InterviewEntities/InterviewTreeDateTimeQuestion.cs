using System;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeDateTimeQuestion : BaseInterviewQuestion
    {
        public InterviewTreeDateTimeQuestion() : base(InterviewQuestionType.DateTime){}

        private DateTimeAnswer answer;
        public InterviewTreeDateTimeQuestion(object answer, bool isTimestamp):base (InterviewQuestionType.DateTime)
        {
            this.IsTimestamp = isTimestamp;
            this.answer = answer == null ? null : DateTimeAnswer.FromDateTime(Convert.ToDateTime(answer));
        }

        public override bool IsAnswered() => this.answer != null;

        public bool IsTimestamp { get; private set; }

        public override AbstractAnswer Answer => this.answer;

        public DateTimeAnswer GetAnswer() => this.answer;

        public void SetAnswer(DateTimeAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question)
        {
            return (question as InterviewTreeDateTimeQuestion)?.answer == this.answer;
        }

        public override BaseInterviewQuestion Clone() => (InterviewTreeDateTimeQuestion) this.MemberwiseClone();

        public string UiFormatString => IsTimestamp ? DateTimeFormat.DateWithTimeFormat : DateTimeFormat.DateFormat;

        public override string ToString() => this.answer?.Value.ToString(UiFormatString) ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireDateTimePreloadValueAllowed();
        }
    }
}
