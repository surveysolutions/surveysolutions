using System;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using Main.Core.Utility;
using WB.Core.GenericSubdomains.Utils;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class SurveyDto : DenormalizerRow
    {
        public SurveyDto(Guid id, string questionnaireTitle, long questionnaireVersion, bool allowCensusMode)
        {
            this.Id = id.Combine(questionnaireVersion).FormatGuid();
            this.QuestionnaireId = id.FormatGuid();
            this.SurveyTitle = questionnaireTitle;
            this.QuestionnaireVersion = questionnaireVersion;
            this.AllowCensusMode = allowCensusMode;
        }

        public SurveyDto() {}

        public string SurveyTitle { get; private set; }
        public string QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public bool AllowCensusMode { get; private set; }
    }
}