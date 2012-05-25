using System;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Group
{
    public class PropagateGroupHandler : ICommandHandler<PropagateGroupCommand>
    {
        private ICompleteQuestionnaireUploaderService _questionnaireservice;


        public PropagateGroupHandler(ICompleteQuestionnaireUploaderService _questionnaireservice)
        {
            this._questionnaireservice = _questionnaireservice;
        }

        #region Implementation of ICommandHandler<PropagateGroupCommand>

        public void Handle(PropagateGroupCommand command)
        {
            _questionnaireservice.PropagateGroup(command.CompleteQuestionnaireId,command.PropagationKey,
                                                                          command.GroupPublicKey);


        }

        #endregion
    }
}
