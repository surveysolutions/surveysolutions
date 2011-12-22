using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateCompleteQuestionnaireCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public IEnumerable<CompleteAnswer> CompleteAnswers { get; private set; }
        public string StatusId { get; private set; }



        public UpdateCompleteQuestionnaireCommand(string completeQuestionanireId, 
                                                  IEnumerable<CompleteAnswer> answers, string statusId)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            this.CompleteAnswers = answers;
            this.StatusId = statusId;
        }
    }
}
