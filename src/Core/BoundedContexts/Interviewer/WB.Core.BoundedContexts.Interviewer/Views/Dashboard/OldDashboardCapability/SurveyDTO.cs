using System;
using Cirrious.MvvmCross.Plugins.Sqlite;

namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    [Obsolete]
    public class SurveyDto
    {
        [PrimaryKey]
        public string Id { get; set; }
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