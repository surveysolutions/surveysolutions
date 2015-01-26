using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewQuestion
    {
        public InterviewQuestion()
        {
        }
             
        public InterviewQuestion(Guid id):this()
        {
            this.Id = id;
            this.QuestionState = QuestionState.Valid | QuestionState.Enabled;
        } 

        public Guid Id { get; set; }
        public object Answer { get;  set; }
        public List<InterviewQuestionComment> Comments { get; set; }
        public QuestionState QuestionState { get; set; }

        public bool IsInvalid()
        {
            return !QuestionState.HasFlag(QuestionState.Valid);
        }

        public bool IsDisabled()
        {
            return !QuestionState.HasFlag(QuestionState.Enabled);
        }

        public bool IsFlagged()
        {
            return QuestionState.HasFlag(QuestionState.Flagged);
        }

        public bool IsAnswered()
        {
            return QuestionState.HasFlag(QuestionState.Answered);
        }
    }

    [Flags]
    public enum QuestionState
    {
        Valid = 8,
        Enabled = 1,
        Flagged = 2,
        Answered = 4
    }
}
