using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewLinkedQuestionView : InterviewQuestionView
    {
        public InterviewLinkedQuestionView(IQuestion question, InterviewQuestion answeredQuestion,
            Dictionary<string, string> answersForTitleSubstitution,
            Dictionary<decimal[], string> availableOptions, bool isParentGroupDisabled, decimal[] rosterVector, InterviewStatus interviewStatus)
            : base(question, answeredQuestion, answersForTitleSubstitution, isParentGroupDisabled, rosterVector, interviewStatus)
        {
            this.Options = availableOptions.Select(a => new QuestionOptionView
                {
                    Value = a.Key,
                    Label = a.Value
                }).ToList();
            if (answeredQuestion != null)
                this.Answer = answeredQuestion.Answer;
        }
    }
}
