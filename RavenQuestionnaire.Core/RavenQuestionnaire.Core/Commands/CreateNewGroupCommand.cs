using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewGroupCommand : ICommand
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
        public Guid? ParentGroupPublicKey
        {
            get;
            private set;
        }
        public CreateNewGroupCommand(string groupText, string questionnaireId, Guid? parentGroup)
        {
            this.GroupText = groupText;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.ParentGroupPublicKey = parentGroup;
        }
    }
}
