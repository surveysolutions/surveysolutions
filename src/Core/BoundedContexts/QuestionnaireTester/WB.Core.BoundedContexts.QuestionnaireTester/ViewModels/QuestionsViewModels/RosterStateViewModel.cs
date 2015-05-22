using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RosterStateViewModel : MvxNotifyPropertyChanged
    {
        private readonly IStatefullInterviewRepository interviewRepository;

        public GroupViewModel GroupState { get; private set; }
        public string RosterTitle { get; private set; }

        public RosterStateViewModel(
            IStatefullInterviewRepository interviewRepository,
            GroupViewModel groupState)
        {
            this.interviewRepository = interviewRepository;
            this.GroupState = groupState;
        }

        public void Init(string interviewId, Identity rosterIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);

            var roster = (InterviewRoster)interview.Groups[ConversionHelper.ConvertIdAndRosterVectorToString(rosterIdentity.Id, rosterIdentity.RosterVector)];

            this.RosterTitle = roster.Title;

            this.GroupState.Init(interviewId, rosterIdentity, navigationState);
        }
    }
}