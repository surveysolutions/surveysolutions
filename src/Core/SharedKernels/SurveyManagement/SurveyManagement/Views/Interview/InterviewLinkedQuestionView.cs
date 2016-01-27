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
            Dictionary<Guid, string> variablesMap, Dictionary<string, string> answersForTitleSubstitution,
            Func<Guid, Dictionary<decimal[], string>> getAvailableOptions, bool isParentGroupDisabled, decimal[] rosterVector, InterviewStatus interviewStatus)
            : base(question, answeredQuestion, variablesMap, answersForTitleSubstitution, isParentGroupDisabled, rosterVector, interviewStatus)
        {
            this.Options = getAvailableOptions(question.PublicKey).Select(a => new QuestionOptionView
                {
                    Value = a.Key,
                    Label = a.Value
                }).ToList();
            if (answeredQuestion != null)
                this.Answer = answeredQuestion.Answer;
        }
    }
}
