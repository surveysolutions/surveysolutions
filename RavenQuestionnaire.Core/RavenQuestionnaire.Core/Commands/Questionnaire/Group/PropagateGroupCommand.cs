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

        public PropagateGroupCommand(string completequestionanireId, Guid propagationKey, Guid propagatedGroupPublickey, UserLight executer)
        {
            this.CompleteQuestionnaireId = completequestionanireId;
            this.Executor = executer;
            this.GroupPublicKey = propagatedGroupPublickey;
            this.PropagationKey = propagationKey;
        }
    }
}
