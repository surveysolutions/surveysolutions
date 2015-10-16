using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class SampleUploadView
    {
        public SampleUploadView(Guid questionnaireId, long questionnaireVersion, List<FeaturedQuestionItem> columnListToPreload)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.ColumnListToPreload = columnListToPreload;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public List<FeaturedQuestionItem> ColumnListToPreload { get; private set; }
    }
}