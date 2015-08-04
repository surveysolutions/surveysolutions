using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

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
}
