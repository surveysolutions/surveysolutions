using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

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
            //    this._questionUploader = questionUploader;
        }

        public void Handle(AddNewQuestionCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);
            if (!this._expressionValidator.Execute(questionnaire, command.ConditionExpression))
                return;
            if (!this._expressionValidator.Execute(questionnaire, command.ValidationExpression))
                return;
            var question = questionnaire.AddQuestion(command.QuestionText, command.StataExportCaption,
                                                     command.QuestionType,
                                                     command.ConditionExpression, command.ValidationExpression,
                                                     command.GroupPublicKey);
            question.UpdateAnswerList(command.Answers);
        }
    }
}
