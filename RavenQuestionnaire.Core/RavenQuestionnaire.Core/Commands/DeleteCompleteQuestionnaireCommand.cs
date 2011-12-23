using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class DeleteCompleteQuestionnaireCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; set; }
        public UserLight Executor { get; set; }

        public DeleteCompleteQuestionnaireCommand(string completeQeuestionnaireId, UserLight executor)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQeuestionnaireId);
            Executor = executor;
        }
    }
}
