using System;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateImageCommand : ICommand
    {
        public UpdateImageCommand(string questionnaireId, Guid questionKey, Guid imageKey, string title, string desc, UserLight executor)
        {
            QuestionKey = questionKey;
            ImageKey = imageKey;
            QuestionnaireId = questionnaireId;
            Title = title;
            Description = desc;
            Executor = executor;
        }
        
        public Guid QuestionKey { get; set; }

        public Guid ImageKey { get; set; }

        public string QuestionnaireId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        #region ICommand Members

        public UserLight Executor { get; set; }

        #endregion
    }
}