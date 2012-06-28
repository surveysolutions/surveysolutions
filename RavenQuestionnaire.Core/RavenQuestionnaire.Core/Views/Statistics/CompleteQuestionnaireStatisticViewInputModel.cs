using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class CompleteQuestionnaireStatisticViewInputModel
    {

        public CompleteQuestionnaireStatisticViewInputModel(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }

    }
}
