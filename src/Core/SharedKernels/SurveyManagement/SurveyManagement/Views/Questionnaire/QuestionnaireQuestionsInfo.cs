using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class QuestionnaireQuestionsInfo : IView
    {
        public Dictionary<Guid, string> QuestionIdToVariableMap { get; set; }
    }
}