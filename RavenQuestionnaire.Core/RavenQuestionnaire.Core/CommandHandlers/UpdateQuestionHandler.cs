using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateQuestionHandler:ICommandHandler<UpdateQuestionCommand>
    {
         private IQuestionnaireRepository _questionnaireRepository;
        //private IQuestionUploaderService _questionUploader;
         public UpdateQuestionHandler(IQuestionnaireRepository questionnaireRepository)
         {
             this._questionnaireRepository = questionnaireRepository;
            //this._questionUploader = questionUploader;
        }
        public void Handle(UpdateQuestionCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(command.QuestionnaireId);
            questionnaire.UpdateQuestion(command.QuestionPublicKey, command.QuestionText, command.QuestionType,
                                         command.ConditionExpression,
                                         command.Answers);

            /*   if(command.Answers!=null)
                questionnaire.UpdateAnswerList(command.Answers);*/
        }
    }
}
