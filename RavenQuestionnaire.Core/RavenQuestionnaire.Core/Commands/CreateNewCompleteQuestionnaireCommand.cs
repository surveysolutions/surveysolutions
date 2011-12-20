using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewCompleteQuestionnaireCommand : ICommand
    {
        public string QuestionnaireId { get; private set; }
        public string UserId { get; private set; }
        public IEnumerable<CompleteAnswer> CompleteAnswers { get; private set; }
        public string CompleteQuestionnaireId { get; set; }

        public CreateNewCompleteQuestionnaireCommand(string questionnaireId, IEnumerable<CompleteAnswer> answers, string creatorId)
        {
            if(string.IsNullOrEmpty(questionnaireId))
                throw  new ArgumentNullException("QuestionnaireId can't be null");
            if (string.IsNullOrEmpty(creatorId))
                throw new ArgumentNullException("User id can't be null");
            this.QuestionnaireId =IdUtil.CreateQuestionnaireId(questionnaireId);
            this.CompleteAnswers = answers;
            this.UserId = creatorId;
        }
    }
}
