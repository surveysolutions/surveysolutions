using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace Core.Supervisor.Views.Interview
{
    public class InterviewLinkedQuestionView : InterviewQuestionView
    {
        public InterviewLinkedQuestionView(IQuestion question, InterviewQuestion answeredQuestion,
            Dictionary<Guid, string> variablesMap, Dictionary<string, string> answersForTitleSubstitution,
            Func<Guid, Dictionary<int[], string>> getAvailableOptions)
            : base(question, answeredQuestion, variablesMap, answersForTitleSubstitution)
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
