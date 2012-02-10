using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateGroupCommand : ICommand
    {
        public string GroupText
        {
            get;
            private set;
        }
        public Propagate Paropagateble
        {
            get;
            private set;
        }
        public string QuestionnaireId
        {
            get;
            private set;
        }
        public Guid GroupPublicKey
        {
            get;
            private set;
        }

        public UserLight Executor { get; set; }

        public UpdateGroupCommand(string groupText, Propagate propagateble, string questionnaireId, Guid parentGroup, UserLight executor)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = parentGroup;
            Executor = executor;
        }
    }
}
