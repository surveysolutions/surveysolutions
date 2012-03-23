using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateConditionsCommand : ICommand
    {
        public string QuestionnaireId { get; set; }
        public Dictionary<Guid, string> Conditions
        {
            get;
            private set;
        }
      
        public UserLight Executor { get; set; }

        public UpdateConditionsCommand(string questionnaireId, Dictionary<Guid, string> conditions, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.Conditions = conditions;
            this.Executor = executor;
        }
    }
}
