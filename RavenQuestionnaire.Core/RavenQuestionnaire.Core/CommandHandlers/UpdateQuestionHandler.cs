using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateQuestionHandler:ICommandHandler<UpdateQuestionCommand>
    {
         private IQuestionnaireRepository _questionnaireRepository;
         private IExpressionExecutor<Questionnaire, bool> _expressionValidator;
         public UpdateQuestionHandler(IQuestionnaireRepository questionnaireRepository, IExpressionExecutor<Questionnaire, bool> validator)
         {
             this._questionnaireRepository = questionnaireRepository;
             this._expressionValidator = validator;
         }

        public void Handle(UpdateQuestionCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(command.QuestionnaireId);
            if (!this._expressionValidator.Execute(questionnaire, command.ConditionExpression))
                return;

            questionnaire.UpdateQuestion(command.QuestionPublicKey, command.QuestionText, command.StataExportCaption,
                                        command.QuestionType,
                                         command.ConditionExpression,
                                         command.Instructions,
                                         command.Answers);

            /*   if(command.Answers!=null)
                questionnaire.UpdateAnswerList(command.Answers);*/
        }
    }
}
