using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Question
{
    public class AddNewQuestionHandler : ICommandHandler<AddNewQuestionCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<Entities.Questionnaire, bool> _expressionValidator;
        public AddNewQuestionHandler(IQuestionnaireRepository questionnaireRepository, IExpressionExecutor<Entities.Questionnaire, bool> validator)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._expressionValidator = validator;
        }

        public void Handle(AddNewQuestionCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);
            if (!this._expressionValidator.Execute(questionnaire, command.ConditionExpression))
                return;
            if (!this._expressionValidator.Execute(questionnaire, command.ValidationExpression))
                return;
            var question = questionnaire.AddQuestion(command.qid, command.QuestionText, command.StataExportCaption,
                                      command.QuestionType,command.ConditionExpression, 
                                      command.ValidationExpression, command.Featured, command.Mandatory, command.AnswerOrder,
                                      command.GroupPublicKey, command.Answers, command.PublicKey);
        }
    }
}
