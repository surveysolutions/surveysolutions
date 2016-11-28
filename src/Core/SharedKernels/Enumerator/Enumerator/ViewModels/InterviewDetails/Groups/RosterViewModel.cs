using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class RosterViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        IDisposable,
        IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private string interviewId;
        private NavigationState navigationState;
        public readonly CovariantObservableCollection<GroupViewModel> RosterInstances;

        public Identity Identity { get; private set; }

        public RosterViewModel(IStatefulInterviewRepository interviewRepository,
            IInterviewViewModelFactory interviewViewModelFactory,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.RosterInstances = new CovariantObservableCollection<GroupViewModel>();
        }

        public void Init(string interviewId, Identity entityId, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.Identity = entityId;
            this.navigationState = navigationState;

            this.eventRegistry.Subscribe(this, interviewId);
            
            this.UpdateFromInterview();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                this.UpdateFromInterview();
        }

        private void UpdateFromInterview()
        {
            var statefulInterview = this.interviewRepository.Get(this.interviewId);

            var interviewRosterInstances =
                statefulInterview.GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id).ToList();

            var rosterIdentitiesByViewModels = this.RosterInstances.Select(viewModel => viewModel.Identity).ToList();

            var notChangedRosterInstances = rosterIdentitiesByViewModels.Intersect(interviewRosterInstances).ToList();

            this.mainThreadDispatcher.RequestMainThreadAction(
                () => this.UpdateViewModels(rosterIdentitiesByViewModels, notChangedRosterInstances, interviewRosterInstances));
        }

        private void UpdateViewModels(List<Identity> rosterIdentitiesByViewModels, List<Identity> notChangedRosterInstances,
            List<Identity> interviewRosterInstances)
        {
            var removedRosterInstances = rosterIdentitiesByViewModels.Except(notChangedRosterInstances).ToList();
            var addedRosterInstances = interviewRosterInstances.Except(notChangedRosterInstances).ToList();

            foreach (var removedRosterInstance in removedRosterInstances)
            {
                var rosterInstanceViewModel = this.RosterInstances.FirstOrDefault(vm => vm.Identity.Equals(removedRosterInstance));
                rosterInstanceViewModel.DisposeIfDisposable();
                this.RosterInstances.Remove(rosterInstanceViewModel);
            }

            foreach (var addedRosterInstance in addedRosterInstances)
            {
                this.RosterInstances.Insert(interviewRosterInstances.IndexOf(addedRosterInstance),
                    this.GetGroupViewModel(addedRosterInstance));
            }
        }

        public void Handle(RosterInstancesAdded @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                this.UpdateFromInterview();
        }

        private GroupViewModel GetGroupViewModel(Identity identity)
        {
            var groupViewModel = this.interviewViewModelFactory.GetNew<GroupViewModel>();
            groupViewModel.Init(this.interviewId, identity, this.navigationState);
            return groupViewModel;
        }
        
        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            foreach (var rosterInstance in this.RosterInstances)
            {
                rosterInstance.DisposeIfDisposable();
            }
        }
    }
}