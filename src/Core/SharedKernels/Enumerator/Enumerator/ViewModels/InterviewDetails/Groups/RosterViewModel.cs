﻿using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
        IViewModelEventHandler<RosterInstancesAdded>,
        IViewModelEventHandler<RosterInstancesRemoved>,
        IDisposable,
        IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IViewModelEventRegistry eventRegistry;
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
            IViewModelEventRegistry eventRegistry)
        {
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.eventRegistry = eventRegistry;
        }

        public void Init(string interviewId, Identity entityId, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.Identity = entityId;
            this.navigationState = navigationState;

            this.RosterInstances = new CovariantObservableCollection<IInterviewEntityViewModel>();
            this.UpdateFromInterview();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                this.UpdateFromInterview();
        }

        public void Handle(RosterInstancesAdded @event)
        {
            if (@event.Instances.Any(rosterInstance => rosterInstance.GroupId == this.Identity.Id))
                this.UpdateFromInterview();
        }

        private void UpdateFromInterview()
        {
            var statefulInterview = this.interviewRepository.Get(this.interviewId);

            var interviewRosterInstances = statefulInterview
                .GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id)
                .ToList();

            this.UpdateViewModels(interviewRosterInstances);
            this.UpdateUi();
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

        private void UpdateUi() => this.InvokeOnMainThread(() => this.RosterInstances.ReplaceWith(this.synchronizedItems));

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

            this.RosterInstances?.ForEach(viewModel => viewModel.DisposeIfDisposable());
        }
    }
}
