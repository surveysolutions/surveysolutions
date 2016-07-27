using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class DetailsViewModel
    {
        public InterviewDetailsFilter Filter { get; set; }
        public Guid? SelectedGroupId { get; set; }
        public decimal[] SelectedGroupRosterVector { get; set; }
        public InterviewDetailsView InterviewDetails { get; set; }
        public IEnumerable<InterviewGroupView> FilteredGroups { get; set; }
        public ChangeStatusView History { get; set; }
        public DetailsStatisticView Statistic { get; set; }
        public bool HasUnprocessedSyncPackages { get; set; }

        public IReadOnlyCollection<InterviewTranslationView> Translations { set; get; }
    }
}
