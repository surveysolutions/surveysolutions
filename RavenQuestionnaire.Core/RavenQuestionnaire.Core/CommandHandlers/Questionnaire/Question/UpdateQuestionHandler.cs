using System;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Question
{
    public class UpdateQuestionHandler : ICommandHandler<UpdateQuestionCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<Entities.Questionnaire, bool> _expressionValidator;
        private IFileRepository _fileRepository;
        public UpdateQuestionHandler(IQuestionnaireRepository questionnaireRepository, IExpressionExecutor<Entities.Questionnaire, bool> validator, IFileRepository fileRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._expressionValidator = validator;
            this._fileRepository = fileRepository;
        }

        public void Handle(UpdateQuestionCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(command.QuestionnaireId);
            if (!this._expressionValidator.Execute(questionnaire, command.ConditionExpression))
                return;

            foreach (var answer in command.Answers)
            {
                if (answer.AnswerType == AnswerType.Image && !string.IsNullOrEmpty(answer.AnswerImage))
                {
                    var file = _fileRepository.Load(answer.AnswerImage).GetInnerDocument();

                    answer.Image = new Image()
                                       {
                                           CreationDate = file.CreationDate,
                                           Description = file.Description,
                                           Height = file.Height,
                                           Width = file.Width,
                                           OriginalBase64 = file.Filename,
                                           PublicKey = Guid.NewGuid(),
                                           ThumbnailBase = file.Thumbnail,
                                           ThumbnailHeight = file.ThumbnailHeight,
                                           ThumbnailWidth = file.ThumbnailWidth,
                                           Title = file.Title
                                       };
                }
                else
                {
                    answer.Image = null;
                }
            }
            questionnaire.UpdateQuestion(command.QuestionPublicKey, command.QuestionText, command.StataExportCaption,
                                        command.QuestionType,
                                         command.ConditionExpression,command.ValidationExpression,
                                         command.Instructions,command.AnswerOrder,
                                         command.Answers);

            /*   if(command.Answers!=null)
                questionnaire.UpdateAnswerList(command.Answers);*/
        }
    }
}
