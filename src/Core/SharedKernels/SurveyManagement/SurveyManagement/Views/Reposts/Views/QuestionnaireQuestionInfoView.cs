using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class QuestionnaireQuestionInfoView
    {
        public QuestionnaireQuestionInfoView()
        {
            this.Variables = new QuestionnaireQuestionInfoItem[0];
        }
        public IEnumerable<QuestionnaireQuestionInfoItem> Variables { get; set; }
    }

    public class QuestionnaireQuestionInfoItem
    {
        public string Variable { get; set; }
        public QuestionType Type { get; set; }
        public Guid Id { get; set; }
    }
}
