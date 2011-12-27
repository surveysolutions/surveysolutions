using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateAnswerInCompleteQuestionnaireCommand: ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public CompleteAnswer[] CompleteAnswers { get; private set; }
        public Guid? Group { get; private set; }


        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionanireId, Guid? group,
                                                  CompleteAnswer[] answers)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            if (group != Guid.Empty)
                this.Group = group;
            this.CompleteAnswers = answers;
        }
    }
}
