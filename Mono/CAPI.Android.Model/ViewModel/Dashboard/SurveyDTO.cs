using System;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using WB.Core.GenericSubdomains.Utils;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class SurveyDto : DenormalizerRow
    {
        public SurveyDto(Guid id, string surveyTitle, long surveyVersion)
        {
            this.Id = id.FormatGuid();
            this.SurveyTitle = surveyTitle;
            this.TemplateMaxVersion = surveyVersion;
        }

        public SurveyDto() {}

        public string SurveyTitle { get; private set; }

        public long TemplateMaxVersion { get; private set; }
    }
}