using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [Serializable]
    public class CreateCompleteQuestionnaireCommand : CommandBase
    {
        public CreateCompleteQuestionnaireCommand(Guid completedQuestionnaireId, Guid questionnaireId, UserLight creator)
        {
            this.CompleteQuestionnaireId = completedQuestionnaireId;
            this.QuestionnaireId = questionnaireId;
            this.Creator = creator;
        }

        public Guid CompleteQuestionnaireId { get; set; }

        public Guid QuestionnaireId { get; set; }
     
        public UserLight Creator { get; set; }
    }
}