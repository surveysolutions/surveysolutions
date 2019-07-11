﻿using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class FlatRosterViewModel : MvxNotifyPropertyChanged,
        IViewModelEventHandler<RosterInstancesAdded>,
        IViewModelEventHandler<RosterInstancesRemoved>,
        IInterviewEntityViewModel,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;
        private string interviewId;
        private NavigationState navigationState;
        private readonly CompositeCollection<ICompositeEntity> rosterInstances;
        private readonly Dictionary<Identity, CompositeCollection<ICompositeEntity>> shownRosterInstances;

        public FlatRosterViewModel(IStatefulInterviewRepository interviewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IViewModelEventRegistry eventRegistry,
            ICompositeCollectionInflationService compositeCollectionInflationService)
        {
            this.interviewRepository = interviewRepository;
            this.viewModelFactory = viewModelFactory;
            this.eventRegistry = eventRegistry;
            this.compositeCollectionInflationService = compositeCollectionInflationService;
            this.rosterInstances = new CompositeCollection<ICompositeEntity>();
            this.shownRosterInstances = new Dictionary<Identity, CompositeCollection<ICompositeEntity>>();
        }

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.navigationState = navigationState;
            this.Identity = entityIdentity;
            this.eventRegistry.Subscribe(this, interviewId);

            UpdateFromInterview();
        }

        private void UpdateFromInterview()
        {
            var statefulInterview = this.interviewRepository.Get(this.interviewId);
            var interviewRosterInstances = statefulInterview
                .GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id)
                .ToList();

            var rosterIdentitiesByViewModels = this.shownRosterInstances.Select(kv => kv.Key).ToList();
            var notChangedRosterInstances = rosterIdentitiesByViewModels.Intersect(interviewRosterInstances).ToList();

            var removedRosterInstances = rosterIdentitiesByViewModels.Except(notChangedRosterInstances).ToList();
            var addedRosterInstances = interviewRosterInstances.Except(notChangedRosterInstances).ToList();

            if (removedRosterInstances.Count == 0 && addedRosterInstances.Count == 0)
                return;

            foreach (var removedRosterInstance in removedRosterInstances)
            {
                if (!this.shownRosterInstances.ContainsKey(removedRosterInstance)) continue;

                var collection = this.shownRosterInstances[removedRosterInstance];
                collection.ForEach(viewModel => viewModel.DisposeIfDisposable());

                this.shownRosterInstances.Remove(removedRosterInstance);
                InvokeOnMainThread(() =>
                {
                    rosterInstances.RemoveCollection(collection);
                });
            }

            foreach (var addedRosterInstance in addedRosterInstances)
            {
                if (this.isDisposed) return;

                InsertRosterInstance(interviewRosterInstances.IndexOf(addedRosterInstance), addedRosterInstance, statefulInterview);
            } 
        }
        
        private void InsertRosterInstance(int rosterIndex, Identity interviewRosterInstance, IStatefulInterview statefulInterview)
        {
            var interviewEntityViewModel = this.viewModelFactory.GetNew<FlatRosterTitleViewModel>();
            interviewEntityViewModel.Init(interviewId, interviewRosterInstance, navigationState);
            var titleCollection = new CovariantObservableCollection<ICompositeEntity>(interviewEntityViewModel.ToEnumerable());

            var underlyingInterviewerEntities = statefulInterview.GetUnderlyingInterviewerEntities(interviewRosterInstance)
                .Select(x => this.viewModelFactory.GetEntity(x, interviewId, navigationState));

            CompositeCollection<ICompositeEntity> inflatedChildren =
                this.compositeCollectionInflationService.GetInflatedCompositeCollection(underlyingInterviewerEntities);
            inflatedChildren.InsertCollection(0, titleCollection);

            InvokeOnMainThread(() =>
            {
                rosterInstances.InsertCollection(rosterIndex, inflatedChildren);
            });

            shownRosterInstances[interviewRosterInstance] = inflatedChildren;
        }

        public CompositeCollection<ICompositeEntity> RosterInstances => rosterInstances;

        public void Handle(RosterInstancesAdded @event)
        {
            if(@event.Instances.Any(x => x.GroupId == this.Identity.Id))
                UpdateFromInterview();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            if(@event.Instances.Any(x => x.GroupId == this.Identity.Id))
                UpdateFromInterview();
        }

        private bool isDisposed;
        public void Dispose()
        {
            if (this.isDisposed) return;

            this.isDisposed = true;
            this.eventRegistry.Unsubscribe(this);

            this.RosterInstances?.ForEach(viewModel => viewModel.DisposeIfDisposable());
            this.RosterInstances?.Clear();
            this.shownRosterInstances?.Clear();
        }
    }
}
