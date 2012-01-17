using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class FlowGraphViewInputModel
    {
        public FlowGraphViewInputModel(Guid publicKey, string questionnaireId)
        {
            PublicKey = publicKey;
            QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }

        public string QuestionnaireId { get; set; }
        public Guid PublicKey { get; private set; }
    }
}
