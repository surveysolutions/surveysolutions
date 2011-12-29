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
        public string ResponsibleId { get; private set; }

        public UserLight Executor { get; set; }


        public UpdateCompleteQuestionnaireCommand(string completeQuestionanireId, 
                                                  IEnumerable<CompleteAnswer> answers, string statusId, string responsible,
                                                    UserLight executor)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            this.CompleteAnswers = answers;
            this.StatusId = statusId;
            this.ResponsibleId = responsible;
            this.Executor = executor;

        }
    }
}
