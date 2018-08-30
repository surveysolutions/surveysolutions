using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
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
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        private string interviewId;
        private NavigationState navigationState;
        private readonly CovariantObservableCollection<GroupViewModel> rosterInstances;
        public IObservableCollection<GroupViewModel> RosterInstances => this.rosterInstances;

        public Identity Identity { get; private set; }

        public RosterViewModel(IStatefulInterviewRepository interviewRepository,
            IInterviewViewModelFactory interviewViewModelFactory,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.rosterInstances = new CovariantObservableCollection<GroupViewModel>();
        }

        public void Init(string interviewId, Identity entityId, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.Identity = entityId;
            this.navigationState = navigationState;

            this.eventRegistry.Subscribe(this, interviewId);
            
            this.UpdateFromInterviewAsync();
        }

        public async void Handle(RosterInstancesRemoved @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                await this.UpdateFromInterviewAsync();
        }

        private async Task UpdateFromInterviewAsync()
        {
            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                var statefulInterview = this.interviewRepository.Get(this.interviewId);

                var interviewRosterInstances =
                    statefulInterview.GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id).ToList();

                var rosterIdentitiesByViewModels = this.RosterInstances.Select(viewModel => viewModel.Identity).ToList();
                var notChangedRosterInstances = rosterIdentitiesByViewModels.Intersect(interviewRosterInstances).ToList();

                var removedRosterInstances = rosterIdentitiesByViewModels.Except(notChangedRosterInstances).ToList();
                var addedRosterInstances = interviewRosterInstances.Except(notChangedRosterInstances).ToList();

                this.UpdateViewModels(removedRosterInstances, addedRosterInstances, interviewRosterInstances);
            });
        }

        private void UpdateViewModels(List<Identity> removedRosterInstances, List<Identity> addedRosterInstances,
            List<Identity> interviewRosterInstances)
        {
            foreach (var removedRosterInstance in removedRosterInstances)
            {
                var rosterInstanceViewModel = this.RosterInstances.FirstOrDefault(vm => vm.Identity.Equals(removedRosterInstance));
                rosterInstanceViewModel.DisposeIfDisposable();
                this.rosterInstances.Remove(rosterInstanceViewModel);
            }

            foreach (var addedRosterInstance in addedRosterInstances)
            {
                this.rosterInstances.Insert(interviewRosterInstances.IndexOf(addedRosterInstance),
                    this.GetGroupViewModel(addedRosterInstance));
            }
        }

        public async void Handle(RosterInstancesAdded @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                await this.UpdateFromInterviewAsync();
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

            foreach (var rosterInstance in this.RosterInstances.ToList())
            {
                rosterInstance.DisposeIfDisposable();
            }
        }
    }
}
