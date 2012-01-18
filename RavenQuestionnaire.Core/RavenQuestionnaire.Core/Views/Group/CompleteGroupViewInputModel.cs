using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class CompleteGroupViewInputModel
    {
        public CompleteGroupViewInputModel(Guid? propagationkey, Guid? publicKey, string questionnaireId)
        {
            PropagationKey = propagationkey;
            PublicKey = publicKey;
            QuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(questionnaireId);
        }

        public string QuestionnaireId { get; set; }
        public Guid? PublicKey { get; private set; }
        public Guid? PropagationKey { get; private set; }
    }
}
