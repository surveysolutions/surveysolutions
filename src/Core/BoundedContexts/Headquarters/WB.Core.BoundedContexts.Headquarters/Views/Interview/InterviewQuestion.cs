using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewQuestion
    {
        public InterviewQuestion()
        {
            this.FailedValidationConditions = new List<FailedValidationCondition>();
            this.FailedWarningConditions = new List<FailedValidationCondition>();
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
        public IReadOnlyList<FailedValidationCondition> FailedValidationConditions { get; set; }
        public IReadOnlyList<FailedValidationCondition> FailedWarningConditions { get; set; }

        public bool IsReadonly()
        {
            return this.QuestionState.HasFlag(QuestionState.Readonly);
        }

        public bool IsInvalid()
        {
            return !this.QuestionState.HasFlag(QuestionState.Valid);
        }

        public bool IsDisabled()
        {
            return !this.QuestionState.HasFlag(QuestionState.Enabled);
        }

        public bool IsFlagged()
        {
            return this.QuestionState.HasFlag(QuestionState.Flagged);
        }

        public bool IsAnswered()
        {
            return this.QuestionState.HasFlag(QuestionState.Answered);
        }
    }
}
