using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewGroupViewModel : OverviewGroup
    {
        public OverviewGroupViewModel(InterviewTreeGroup treeNode) : base(treeNode)
        {
        }

        public void Init(IStatefulInterview interview, DynamicTextViewModel groupTitle, Identity entityIdentity)
        {
            this.GroupTitle = groupTitle;
            this.GroupTitle.Init(interview.Id.FormatGuid(), entityIdentity);
            this.RosterInstanceTitle = interview.GetRosterTitle(entityIdentity);
        }

        public DynamicTextViewModel GroupTitle { get; set; }

        public string RosterInstanceTitle { get; set; }

        public override void Dispose()
        {
            this.GroupTitle.Dispose();           
            base.Dispose();
        }
    }
}
