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
    public class PropagateGroupHandler : ICommandHandler<PropagateGroupCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;

        public PropagateGroupHandler(ICompleteQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        #region Implementation of ICommandHandler<PropagateGroupCommand>

        public void Handle(PropagateGroupCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            entity.Add(entity.Find<CompleteGroup>(command.GroupPublicKey), null);
        }

        #endregion
    }
}
