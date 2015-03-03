using System;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using WB.Core.GenericSubdomains.Utils;

namespace WB.UI.Capi.ViewModel.Dashboard
{
    public class SurveyDto : DenormalizerRow
    {
        public SurveyDto(Guid id, string questionnaireTitle, long questionnaireVersion, bool allowCensusMode)
        {
            this.Id = GetStorageId(id, questionnaireVersion);
            this.QuestionnaireId = id.FormatGuid();
            this.SurveyTitle = questionnaireTitle;
            this.QuestionnaireVersion = questionnaireVersion;
            this.AllowCensusMode = allowCensusMode;
        }

        public SurveyDto() {}

        public string SurveyTitle { get; private set; }
        public string QuestionnaireId { get; private set; }

        public long QuestionnaireVersion
        {
            get
            {
                if (questionnaireVersion == 0)
                {
                    return TemplateMaxVersion;
                }
                return questionnaireVersion;
            }
            private set
            {
                questionnaireVersion = value;
            }
        }

        private long questionnaireVersion;
        public bool AllowCensusMode { get; private set; }
        public long TemplateMaxVersion { get; private set; }

        public static string GetStorageId(Guid id, long questionnaireVersion)
        {
            return id.Combine(questionnaireVersion).FormatGuid();
        }
    }
}