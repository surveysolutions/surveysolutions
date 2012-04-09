using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class ValidateGroupCommandHandler : ICommandHandler<ValidateGroupCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;
        private IValildationService _validator;

        public ValidateGroupCommandHandler(ICompleteQuestionnaireRepository questionnaireRepository, IValildationService validator)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._validator = validator;
        }

        #region Implementation of ICommandHandler<in ValidateGroupCommand>

        public void Handle(ValidateGroupCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.QuestionnaireId);
            this._validator.Validate(entity, command.GroupPublicKey, command.PropagationKeyKey);
        }

        #endregion
    }
}
