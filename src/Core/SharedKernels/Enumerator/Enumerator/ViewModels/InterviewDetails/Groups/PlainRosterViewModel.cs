using System;
using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class PlainRosterViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<YesNoQuestionAnswered>,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private string interviewId;
        private NavigationState navigationState;

        public PlainRosterViewModel(IStatefulInterviewRepository interviewRepository,
            IInterviewViewModelFactory viewModelFactory)
        {
            this.interviewRepository = interviewRepository;
            this.viewModelFactory = viewModelFactory;
            this.RosterInstances = new CovariantObservableCollection<IInterviewEntityViewModel>();
        }

        //public void Handle(RosterInstancesTitleChanged @event)
        //{
        //    throw new NotImplementedException();
        //}

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.navigationState = navigationState;
            this.Identity = entityIdentity;

            UpdateFromInterview();
        }

        private void UpdateFromInterview()
        {
            var statefulInterview = this.interviewRepository.Get(this.interviewId);
            var interviewRosterInstances = statefulInterview.GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id);

            RosterInstances.SuspendCollectionChanged();
            this.RosterInstances.Clear();

            try
            {
                foreach (var interviewRosterInstance in interviewRosterInstances)
                {
                    var interviewEntityViewModel = this.viewModelFactory.GetNew<PlainRosterTitleViewModel>();
                    interviewEntityViewModel.Init(interviewId, interviewRosterInstance, navigationState);
                    this.RosterInstances.Add(interviewEntityViewModel);
                    foreach (var underlyingInterviewerEntity in statefulInterview.GetUnderlyingInterviewerEntities(interviewRosterInstance))
                    {
                        this.RosterInstances.Add(this.viewModelFactory.GetEntity(underlyingInterviewerEntity, interviewId, navigationState));
                    }
                }
            }
            finally
            {
                RosterInstances.ResumeCollectionChanged();
            }
        }

        public CovariantObservableCollection<IInterviewEntityViewModel> RosterInstances { get; }

        public void Handle(RosterInstancesAdded @event)
        {
            //throw new NotImplementedException();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            //throw new NotImplementedException();
        }

        public void Handle(YesNoQuestionAnswered @event)
        {
            //throw new NotImplementedException();
        }

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            //throw new NotImplementedException();
        }
    }
}
