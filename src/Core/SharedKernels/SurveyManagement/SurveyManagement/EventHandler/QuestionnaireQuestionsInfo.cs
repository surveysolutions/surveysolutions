using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
    {
        public class QuestionnaireQuestionsInfo : IView
        {
            public Dictionary<Guid, string> QuestionIdToVariableMap { get; set; }
        }
    }
}