using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    public class DeletePropagatedGroupCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public Guid GroupPublicKey { get; private set; }
        public Guid PropagationPublicKey { get; private set; }

        #region Implementation of ICommand

        public UserLight Executor { get; set; }

        #endregion

        public DeletePropagatedGroupCommand(string completequestionanireId,Guid groupPublicKey, Guid propagationPublicKey,
                                            UserLight executer)
        {
            this.CompleteQuestionnaireId = completequestionanireId;
            this.Executor = executer;
            this.GroupPublicKey = groupPublicKey;
            this.PropagationPublicKey = propagationPublicKey;
        }
    }
}
