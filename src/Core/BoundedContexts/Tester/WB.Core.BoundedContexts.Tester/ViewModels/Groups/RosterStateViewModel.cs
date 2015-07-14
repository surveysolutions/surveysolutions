using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Groups
{
    public class RosterStateViewModel : MvxNotifyPropertyChanged
    {
        private readonly IStatefulInterviewRepository interviewRepository;

        public GroupViewModel GroupState { get; private set; }
        public string RosterTitle { get; private set; }

        public RosterStateViewModel(
            IStatefulInterviewRepository interviewRepository,
            GroupViewModel groupState)
        {
            this.interviewRepository = interviewRepository;
            this.GroupState = groupState;
        }

        public void Init(string interviewId, Identity rosterIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            this.RosterTitle = interview.GetRosterTitle(rosterIdentity);

            this.GroupState.Init(interviewId, rosterIdentity, navigationState);
        }
    }
}