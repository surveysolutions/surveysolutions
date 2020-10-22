using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class SurveyStatisticsReportInputModel : ListViewModelBase
    {
        public string QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? TeamLeadId { get; set; }
        public bool ShowTeamLead { get; set; }
        public bool ShowTeamMembers { get; set; }
        public int? MinAnswer { get; set; }
        public int? MaxAnswer { get; set; }

        public Guid? ConditionalQuestionId { get; set; }
        public long[] Condition { get; set; }
        public bool Pivot { get; set; }
        public string[] Columns { get; set; }

        public string[] Statuses { get; set; }
    }
}
