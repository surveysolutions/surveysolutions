using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    public class ValidateGroupCommand : ICommand
    {
        #region Implementation of ICommand

        public UserLight Executor { get; set; }

        #endregion

        public string QuestionnaireId { get; private set; }
        public Guid? GroupPublicKey { get; private set; }
        public Guid? PropagationKeyKey { get; private set; }

        public ValidateGroupCommand(string questionnaireId, Guid? groupKey, Guid? propagationKey, UserLight executor)
        {
            this.QuestionnaireId = questionnaireId;
            this.GroupPublicKey = groupKey;
            this.PropagationKeyKey = propagationKey;
            this.Executor = executor;
        }
    }
}
