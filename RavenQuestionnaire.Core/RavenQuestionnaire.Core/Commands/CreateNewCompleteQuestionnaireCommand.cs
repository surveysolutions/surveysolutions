using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewCompleteQuestionnaireCommand : ICommand
    {
        public string QuestionnaireId { get; private set; }
        public UserLight Creator { get; private set; }
        public string CompleteQuestionnaireId { get; set; }
        public SurveyStatus Status { set; get; }

        public UserLight Executor { get; set; }

        public CreateNewCompleteQuestionnaireCommand(string questionnaireId, 
            UserLight creator, 
            SurveyStatus status,
            UserLight executor)
        {
            if(string.IsNullOrEmpty(questionnaireId))
                throw  new ArgumentNullException("QuestionnaireId can't be null");
            if (string.IsNullOrEmpty(creator.Id))
                throw new ArgumentNullException("User id can't be null");
            this.QuestionnaireId =IdUtil.CreateQuestionnaireId(questionnaireId);
            this.Creator = creator;
            this .Status = status;
            this.Executor = executor;
        }
    }
}
