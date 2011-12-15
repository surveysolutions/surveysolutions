using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewInputModel
    {
        public CompleteQuestionnaireViewInputModel(string id)
        {
            CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(id);
        }

        public string CompleteQuestionnaireId { get; private set; }
    }
}
