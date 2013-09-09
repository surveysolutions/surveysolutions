using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace Core.Supervisor.Views.Interview
{
    public class InterviewQuestionView
    {
        public InterviewQuestionView(ICompleteQuestion question, InterviewQuestion answeredQuestion)
        {
            Id = question.PublicKey;
            Title = question.QuestionText;
            QuestionType = question.QuestionType;
            IsMandatory = question.Mandatory;
            IsFeatured = question.Featured;
            IsCapital = question.Capital;
            ValidationMessage = question.ValidationMessage;
            ValidationExpression = question.ValidationExpression;
         
            if (answeredQuestion == null) return;

            IsAnswered = answeredQuestion.IsAnswered;
            IsEnabled = answeredQuestion.Enabled;
            IsFlagged = answeredQuestion.Flagged;
            Comments = answeredQuestion.Comments;
            IsValid = answeredQuestion.Valid;
            Scope = question.QuestionScope;
            Answer = answeredQuestion.Answer;
        }

        public QuestionScope Scope { get; set; }

        public bool IsAnswered { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public QuestionType QuestionType { get; set; }

        public bool IsMandatory { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsCapital { get; set; }
        public bool IsValid { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFlagged { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }

        public int[] PropagationVector { get; set; }

        public List<InterviewQuestionComment> Comments { get; set; }

        public object Answer { get; set; }
    }
}