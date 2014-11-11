using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [Serializable]
    public class CreateInterviewWithFeaturedQuestionsCommand : CommandBase
    {
        public CreateInterviewWithFeaturedQuestionsCommand(Guid interviewId, Guid questionnaireId, UserLight creator, UserLight responsible, List<QuestionAnswer> featuredAnswers)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.Creator = creator;
            this.Responsible = responsible;
            this.FeaturedAnswers = featuredAnswers;
        }

        public Guid QuestionnaireId { get; set; }

        public Guid InterviewId { get; set; }

        public UserLight Creator { get; set; }

        public UserLight Responsible { get; set; }
        
        public List<QuestionAnswer> FeaturedAnswers { get; set; }
    }
}