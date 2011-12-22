using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public UpdateGroupCommand(string groupText, string questionnaireId, Guid parentGroup)
        {
            this.GroupText = groupText;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = parentGroup;
        }
    }
}
