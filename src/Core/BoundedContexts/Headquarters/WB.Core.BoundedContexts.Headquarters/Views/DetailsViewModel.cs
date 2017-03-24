using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class DetailsViewModel
    {
        public InterviewDetailsFilter Filter { get; set; }
        public Identity SelectedGroupId { get; set; }
        public decimal[] SelectedGroupRosterVector { get; set; }
        public InterviewDetailsView InterviewDetails { get; set; }
        public IEnumerable<InterviewEntityView> FilteredEntities { get; set; }
        public ChangeStatusView History { get; set; }
        public DetailsStatisticView Statistic { get; set; }
        public bool HasUnprocessedSyncPackages { get; set; }

        public IReadOnlyCollection<InterviewTranslationView> Translations { set; get; }

        public string QuestionnaireName { get; set; }
        public long QuestionnaireVersion { get; set; }

        public string InterviewKey { get; set; }
    }
}
