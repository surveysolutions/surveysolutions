using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class DeletePropagatedGroupHandler : ICommandHandler<DeletePropagatedGroupCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;

        public DeletePropagatedGroupHandler(ICompleteQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        #region Implementation of ICommandHandler<DeletePropagatedGroupCommand>

        public void Handle(DeletePropagatedGroupCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            //   entity.Remove(new PropagatableCompleteGroup(entity.Find<CompleteGroup>(command.GroupPublicKey)))

            entity.Remove(new PropagatableCompleteGroup(entity.Find<CompleteGroup>(command.GroupPublicKey),
                                                        command.PropagationPublicKey));
        }

        #endregion
    }
}
