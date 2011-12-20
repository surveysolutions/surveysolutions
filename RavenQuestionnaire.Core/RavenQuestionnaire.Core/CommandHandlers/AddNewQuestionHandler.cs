using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class AddNewQuestionHandler : ICommandHandler<AddNewQuestionCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
      //  private IQuestionUploaderService _questionUploader;
        public AddNewQuestionHandler(IQuestionnaireRepository questionnaireRepository/*, IQuestionUploaderService questionUploader*/)
        {
            this._questionnaireRepository = questionnaireRepository;
        //    this._questionUploader = questionUploader;
        }

        public void Handle(AddNewQuestionCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);
            var question = questionnaire.AddQuestion(command.QuestionText, command.QuestionType);
            question.UpdateAnswerList(command.Answers);
            question.SetConditionExpression(command.ConditionExpression);
        }
    }
}
