using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Group
{
    public class DeletePropagatedGroupHandler : ICommandHandler<DeletePropagatedGroupCommand>
    {
        private ICompleteQuestionnaireUploaderService _questionnaireservice;

        public DeletePropagatedGroupHandler(ICompleteQuestionnaireUploaderService questionnaireservice)
        {
            this._questionnaireservice = questionnaireservice;
        }

        #region Implementation of ICommandHandler<DeletePropagatedGroupCommand>

        public void Handle(DeletePropagatedGroupCommand command)
        {
            _questionnaireservice.RemovePropagatedGroup(command.CompleteQuestionnaireId, command.GroupPublicKey,
                                                        command.PropagationPublicKey);
        }

        #endregion
    }
}
