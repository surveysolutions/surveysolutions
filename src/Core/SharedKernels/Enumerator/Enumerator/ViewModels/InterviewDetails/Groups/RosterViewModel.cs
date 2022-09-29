using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class RosterViewModel : MvxNotifyPropertyChanged,
        IAsyncViewModelEventHandler<RosterInstancesAdded>,
        IAsyncViewModelEventHandler<RosterInstancesRemoved>,
        IDisposable,
        IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher;
        private string interviewId;
        private NavigationState navigationState;

        private readonly SynchronizedList<IInterviewEntityViewModel> synchronizedItems = new SynchronizedList<IInterviewEntityViewModel>();

        private CovariantObservableCollection<IInterviewEntityViewModel> rosterInstances;
        public CovariantObservableCollection<IInterviewEntityViewModel> RosterInstances
        {
            get => this.rosterInstances;
            set => this.RaiseAndSetIfChanged(ref this.rosterInstances, value);
        }

        public Identity Identity { get; private set; }

        public RosterViewModel(IStatefulInterviewRepository interviewRepository,
            IInterviewViewModelFactory interviewViewModelFactory,
            IViewModelEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.eventRegistry = eventRegistry;
            this.mainThreadAsyncDispatcher = mainThreadAsyncDispatcher;
        }

        public void Init(string interviewId, Identity entityId, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.Identity = entityId;
            this.navigationState = navigationState;

            this.RosterInstances = new CovariantObservableCollection<IInterviewEntityViewModel>();
            this.UpdateFromInterview().WaitAndUnwrapException();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        public async Task HandleAsync(RosterInstancesRemoved @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                await this.UpdateFromInterview();
        }

        public async Task HandleAsync(RosterInstancesAdded @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                await this.UpdateFromInterview();
        }

        private async Task UpdateFromInterview()
        {
            if (this.isDisposed) return;
            
            var statefulInterview = this.interviewRepository.Get(this.interviewId);

            var interviewRosterInstances = statefulInterview
                .GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id)
                .ToList();

            this.UpdateViewModels(interviewRosterInstances);

            await mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(() => this.RosterInstances.ReplaceWith(this.synchronizedItems));
        }

        private void UpdateViewModels(IList<Identity> interviewRosterInstances)
        {
            var rosterIdentitiesByViewModels = this.synchronizedItems.Select(viewModel => viewModel.Identity).ToList();
            var notChangedRosterInstances = rosterIdentitiesByViewModels.Intersect(interviewRosterInstances).ToList();

            var removedRosterInstances = rosterIdentitiesByViewModels.Except(notChangedRosterInstances).ToList();
            var addedRosterInstances = interviewRosterInstances.Except(notChangedRosterInstances).ToList();

            foreach (var removedRosterInstance in removedRosterInstances)
            {
                var removedViewModel = this.synchronizedItems.FirstOrDefault(vm => vm.Identity.Equals(removedRosterInstance));
                if (removedViewModel == null) continue;

                removedViewModel.DisposeIfDisposable();
                this.synchronizedItems.Remove(removedViewModel);
            }

            foreach (var addedRosterInstance in addedRosterInstances)
            {
                if (this.isDisposed) return;
                this.synchronizedItems.Insert(interviewRosterInstances.IndexOf(addedRosterInstance), this.GetGroupViewModel(addedRosterInstance));
            }
        }

        private GroupViewModel GetGroupViewModel(Identity identity)
        {
            var groupViewModel = this.interviewViewModelFactory.GetNew<GroupViewModel>();
            groupViewModel.Init(this.interviewId, identity, this.navigationState);
            return groupViewModel;
        }

        private bool isDisposed;
        public void Dispose()
        {
            if (this.isDisposed) return;

            this.isDisposed = true;
            this.eventRegistry.Unsubscribe(this);

            this.synchronizedItems?.ForEach(viewModel => viewModel.DisposeIfDisposable());
            this.RosterInstances?.ForEach(viewModel => viewModel.DisposeIfDisposable());
        }
    }
}
