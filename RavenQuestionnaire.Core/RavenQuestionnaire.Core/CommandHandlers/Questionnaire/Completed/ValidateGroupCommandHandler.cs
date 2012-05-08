#region

using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

#endregion

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class ValidateGroupCommandHandler : ICommandHandler<ValidateGroupCommand>
    {
        private readonly ICompleteQuestionnaireRepository _questionnaireRepository;
        private readonly IValildationService _validator;

        public ValidateGroupCommandHandler(ICompleteQuestionnaireRepository questionnaireRepository,
                                           IValildationService validator)
        {
            _questionnaireRepository = questionnaireRepository;
            _validator = validator;
        }

        #region Implementation of ICommandHandler<in ValidateGroupCommand>

        public void Handle(ValidateGroupCommand command)
        {
            var entity = _questionnaireRepository.Load(command.QuestionnaireId);
            bool isValid = _validator.Validate(entity, command.GroupPublicKey, command.PropagationKeyKey);
            entity.GetInnerDocument().IsValid = isValid;
        }

        #endregion
    }
}