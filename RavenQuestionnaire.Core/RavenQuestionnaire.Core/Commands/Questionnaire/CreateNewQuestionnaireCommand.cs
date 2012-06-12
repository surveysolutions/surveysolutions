using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    public class CreateNewQuestionnaireCommand : ICommand
    {
        public Guid QuestionnaireGuid { get; private set; }

        public string Title
        {
            get;
            private set;
        }

        public string DefaultStatusGroupId
        {
            get;
            private set;
        }

        public CreateNewQuestionnaireCommand(string title, string defaultStatusGroupId, Guid questionnaireGuid,
            UserLight executor)
        {
            this.Title = title;
            this.QuestionnaireGuid = questionnaireGuid;
            this.Executor = executor;

            this.DefaultStatusGroupId = defaultStatusGroupId == null? null : IdUtil.CreateStatusId(defaultStatusGroupId);
        }

        public UserLight Executor
        {
            get;
            set;
        }
    }
}
