using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    public class PropagateGroupCommand: ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public Guid GroupPublicKey { get; private set; }
        public Guid PropagationKey { get; private set; }
        #region Implementation of ICommand

        public UserLight Executor { get; set; }

        #endregion

        public PropagateGroupCommand(string completeQuestionnaireId, Guid propagationKey, Guid groupPublicKey, UserLight executer)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.Executor = executer;
            this.GroupPublicKey = groupPublicKey;
            this.PropagationKey = propagationKey;
        }
    }
}
