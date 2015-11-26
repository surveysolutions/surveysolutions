using System;
using WB.UI.Interviewer.Implementations.DenormalizerStorage;

namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    [Obsolete]
    public class SurveyDto : DenormalizerRow
    {
        public string SurveyTitle { get; set; }
        public string QuestionnaireId { get; set; }

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
        public bool AllowCensusMode { get; set; }
        public long TemplateMaxVersion { get; set; }
    }
}