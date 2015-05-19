using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public abstract class BaseInterviewAnswer
    {
        protected BaseInterviewAnswer()
        {
            this.IsEnabled = true;
            this.IsValid = true;
        }

        protected BaseInterviewAnswer(Guid id, decimal[] rosterVector)
            : this()
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }

        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }

        public bool IsValid { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsAnswered { get; set; }

        public string InterviewerComment { get; set; }

        public abstract void RemoveAnswer();
    }
}