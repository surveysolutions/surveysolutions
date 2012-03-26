using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire
{
    public class UpdateConditionsHandler : ICommandHandler<UpdateConditionsCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<Entities.Questionnaire, bool> _expressionValidator;
        public UpdateConditionsHandler(IQuestionnaireRepository questionnaireRepository, IExpressionExecutor<Entities.Questionnaire, bool> validator)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._expressionValidator = validator;
        }

        public void Handle(UpdateConditionsCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(command.QuestionnaireId);
            foreach (var condition in command.Conditions)
            {
                if (!this._expressionValidator.Execute(questionnaire, condition.Value))
                    continue;
                questionnaire.UpdateConditionExpression(condition.Key, condition.Value);
            }
        }
    }
}
