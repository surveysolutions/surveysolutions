using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class UpdateAnswerInCompleteQuestionnaireHandler :
        ICommandHandler<UpdateAnswerInCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireUploaderService _completeQuestionnaireUploader;

        public UpdateAnswerInCompleteQuestionnaireHandler(
            ICompleteQuestionnaireUploaderService completeQuestionnaireUploader)
        {
            this._completeQuestionnaireUploader = completeQuestionnaireUploader;
        }

        public void Handle(UpdateAnswerInCompleteQuestionnaireCommand command)
        {
            this._completeQuestionnaireUploader.AddCompleteAnswer(command.CompleteQuestionnaireId,
                                                                  command.QuestionPublickey, command.Propagationkey,
                                                                  command.CompleteAnswer ?? command.CompleteAnswers);
        }
    }
}
