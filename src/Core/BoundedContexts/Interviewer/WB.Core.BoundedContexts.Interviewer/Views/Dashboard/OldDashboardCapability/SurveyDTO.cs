using System;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Interviewer.Implementations.DenormalizerStorage;

namespace WB.UI.Interviewer.ViewModel.Dashboard
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
                if (this.questionnaireVersion == 0)
                {
                    return this.TemplateMaxVersion;
                }
                return this.questionnaireVersion;
            }
            private set
            {
                this.questionnaireVersion = value;
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