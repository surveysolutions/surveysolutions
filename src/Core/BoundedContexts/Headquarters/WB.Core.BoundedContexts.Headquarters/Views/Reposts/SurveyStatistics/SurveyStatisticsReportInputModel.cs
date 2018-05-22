using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class SurveyStatisticsReportInputModel : ListViewModelBase
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public IQuestion Question { get; set; }
        public Guid? TeamLeadId { get; set; }
        public bool ShowTeamLead { get; set; }
        public bool ShowTeamMembers { get; set; }
        public int? MinAnswer { get; set; }
        public int? MaxAnswer { get; set; }

        public IQuestion ConditionalQuestion { get; set; }
        public long[] Condition { get; set; }
        public bool Pivot { get; set; }
        public string[] Columns { get; set; }
    }
}
