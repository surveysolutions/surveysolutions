using System;
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
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class RosterViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<YesNoQuestionAnswered>,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        IDisposable,
        IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly ILiteEventRegistry eventRegistry;
        private string interviewId;
        private NavigationState navigationState;
        public CovariantObservableCollection<IInterviewEntityViewModel> RosterInstances { get; }
        private readonly IQuestionnaireStorage questionnaireRepository;

        public Identity Identity { get; private set; }
        private bool isTriggeredByOrderedMultiQuestion = false;
        private Guid rosterSizeQuestionId = Guid.Empty;

        public RosterViewModel(IStatefulInterviewRepository interviewRepository,
            IInterviewViewModelFactory interviewViewModelFactory,
            ILiteEventRegistry eventRegistry,
            IQuestionnaireStorage questionnaireRepository)
        {
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.eventRegistry = eventRegistry;
            this.questionnaireRepository = questionnaireRepository;
            this.RosterInstances = new CovariantObservableCollection<IInterviewEntityViewModel>();
        }

        public void Init(string interviewId, Identity entityId, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.Identity = entityId;
            this.navigationState = navigationState;

            this.eventRegistry.Subscribe(this, interviewId);

            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.isTriggeredByOrderedMultiQuestion = questionnaire.IsRosterTriggeredByOrderedMultiQuestion(entityId.Id);
            if (this.isTriggeredByOrderedMultiQuestion)
            {
                this.rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(entityId.Id);
            }

            this.IsPlainRoster = questionnaire.IsPlainRoster(entityId.Id);

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

            var interviewRosterInstances = statefulInterview.GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id)
                                                            .ToList();
            if (!IsPlainRoster)
            {
                var rosterIdentitiesByViewModels =
                    this.RosterInstances.Select(viewModel => viewModel.Identity).ToList();
                var notChangedRosterInstances =
                    rosterIdentitiesByViewModels.Intersect(interviewRosterInstances).ToList();

                var removedRosterInstances = rosterIdentitiesByViewModels.Except(notChangedRosterInstances).ToList();
                var addedRosterInstances = interviewRosterInstances.Except(notChangedRosterInstances).ToList();

                this.UpdateViewModels(removedRosterInstances, addedRosterInstances, interviewRosterInstances, this.RosterInstances);
            }
            else
            {
                this.RosterInstances.SuspendCollectionChanged();

                foreach (var interviewRosterInstance in interviewRosterInstances)
                {
                    
                }

                //List<Identity> shouldBeOnUiList = interviewRosterInstances
                //    .SelectMany(x => statefulInterview.GetUnderlyingInterviewerEntities(x))
                //    .ToList();

                //if (shouldBeOnUiList.Count == 0) this.RosterInstances.Clear();
                //else
                //{
                //    this.RosterInstances.SuspendCollectionChanged();

                //    for (int i = 0; i < shouldBeOnUiList.Count; i++)
                //    {
                //        var currentShouldBeOnUi = shouldBeOnUiList[i];
                //        if (i == RosterInstances.Count)
                //        {
                //            this.RosterInstances.Add(this.interviewViewModelFactory.GetEntity(currentShouldBeOnUi, interviewId, navigationState));
                //        }
                //        else
                //        {
                //            var existingOnUi = this.RosterInstances[i].Identity;
                //            if (!currentShouldBeOnUi.Equals(existingOnUi))
                //            {
                //                this.RosterInstances[i].DisposeIfDisposable();
                //                this.RosterInstances.RemoveAt(i);
                //                this.RosterInstances.Insert(i, this.interviewViewModelFactory.GetEntity(currentShouldBeOnUi, interviewId, navigationState));
                //            }
                //        }
                //    }

                //    if (this.RosterInstances.Count > shouldBeOnUiList.Count)
                //    {
                //        for (int i = 0; i < this.RosterInstances.Count - shouldBeOnUiList.Count; i++)
                //        {
                //            var index = this.RosterInstances.Count - 1 - i;
                //            var removedViewModel = this.RosterInstances[index];
                //            removedViewModel.DisposeIfDisposable();
                //            this.RosterInstances.Remove(removedViewModel);
                //        }
                //    }

                //    this.RosterInstances.ResumeCollectionChanged();
                //}
            }
        }

        public bool IsPlainRoster { get; private set; }

        private void UpdateViewModels(List<Identity> removedRosterInstances, List<Identity> addedRosterInstances,
            List<Identity> interviewRosterInstances, 
            CovariantObservableCollection<IInterviewEntityViewModel> target)
        {
            target.SuspendCollectionChanged();

            foreach (var removedRosterInstance in removedRosterInstances)
            {
                var rosterInstanceViewModel =
                    this.RosterInstances.FirstOrDefault(vm => vm.Identity.Equals(removedRosterInstance));
                rosterInstanceViewModel.DisposeIfDisposable();
                target.Remove(rosterInstanceViewModel);
            }

            foreach (var addedRosterInstance in addedRosterInstances)
            {
                target.Insert(interviewRosterInstances.IndexOf(addedRosterInstance),
                    this.GetGroupViewModel(addedRosterInstance));
            }

            // change order
            for (int i = 0; i < interviewRosterInstances.Count; i++)
            {
                var rosterInstanceViewModel =
                    this.RosterInstances.FirstOrDefault(vm => vm.Identity.Equals(interviewRosterInstances[i]));

                if (rosterInstanceViewModel == null)
                    continue;

                target.Remove(rosterInstanceViewModel);
                target.Insert(i, rosterInstanceViewModel);
            }

            target.ResumeCollectionChanged();
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

            foreach (var rosterInstance in this.RosterInstances.ToList())
            {
                rosterInstance.DisposeIfDisposable();
            }
        }

        public void Handle(YesNoQuestionAnswered @event)
        {
            if (!isTriggeredByOrderedMultiQuestion)
                return;

            if (@event.QuestionId != this.rosterSizeQuestionId)
                return;

            this.UpdateFromInterview();
        }

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if (!isTriggeredByOrderedMultiQuestion)
                return;

            if (@event.QuestionId != this.rosterSizeQuestionId)
                return;

            this.UpdateFromInterview();
        }
    }
}
