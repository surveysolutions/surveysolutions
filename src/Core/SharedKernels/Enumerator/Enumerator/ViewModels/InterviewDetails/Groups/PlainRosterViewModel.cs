using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private string interviewId;
        private NavigationState navigationState;
        private readonly CovariantObservableCollection<ICompositeEntity> rosterInstances;
        private List<Identity> shownRosterInstances;

        public PlainRosterViewModel(IStatefulInterviewRepository interviewRepository,
            IInterviewViewModelFactory viewModelFactory,
            ILiteEventRegistry eventRegistry,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.interviewRepository = interviewRepository;
            this.viewModelFactory = viewModelFactory;
            this.eventRegistry = eventRegistry;
            this.compositeCollectionInflationService = compositeCollectionInflationService;
            this.questionnaireStorage = questionnaireStorage;
            this.rosterInstances = new CovariantObservableCollection<ICompositeEntity>();
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
            var interviewRosterInstances = statefulInterview.GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id);
            
            List<ICompositeEntity> uiEntities = new List<ICompositeEntity>();
            foreach (var interviewRosterInstance in interviewRosterInstances)
            {
                var interviewEntityViewModel = this.viewModelFactory.GetNew<PlainRosterTitleViewModel>();
                interviewEntityViewModel.Init(interviewId, interviewRosterInstance, navigationState);
                uiEntities.Add(interviewEntityViewModel);

                var underlyingInterviewerEntities = statefulInterview.GetUnderlyingInterviewerEntities(interviewRosterInstance)
                    .Select(x => this.viewModelFactory.GetEntity(x, interviewId, navigationState));

                CompositeCollection<ICompositeEntity> inflatedChildren = this.compositeCollectionInflationService.GetInflatedCompositeCollection(underlyingInterviewerEntities);
                foreach (var compositeEntity in inflatedChildren)
                {
                    uiEntities.Add(compositeEntity);
                }
            }

            try
            {
                InvokeOnMainThread(() =>
                {
                    rosterInstances.SwitchTo(uiEntities);
                });
            }
            finally
            {
                shownRosterInstances = interviewRosterInstances;
            }
        }

        private bool IsChangedRosterInstances()
        {
            var statefulInterview = this.interviewRepository.Get(this.interviewId);
            var interviewRosterInstances = statefulInterview.GetRosterInstances(this.navigationState.CurrentGroup, this.Identity.Id);
            if(shownRosterInstances?.Count == interviewRosterInstances.Count && shownRosterInstances.SequenceEqual(interviewRosterInstances))
                return false;

            return true;
        }

        private bool IsChangedRosterChildren(Identity[] identities)
        {
            var statefulInterview = this.interviewRepository.Get(this.interviewId);
            var questionnaireDocument = questionnaireStorage.GetQuestionnaireDocument(statefulInterview.QuestionnaireIdentity);
            return identities.Any(identity =>
            {
                var entity = questionnaireDocument.Find<IComposite>(identity.Id);
                if (entity is IConditional conditional && conditional.HideIfDisabled == false)
                    return false;

                var parent = (IGroup)entity.GetParent();
                if (!parent.IsPlainMode)
                    return false;

                return parent.GetParent().PublicKey == this.navigationState.CurrentGroup.Id;
            });
        }

        public IObservableCollection<ICompositeEntity> RosterInstances => rosterInstances;

        public void Handle(RosterInstancesAdded @event)
        {
            if(@event.Instances.Any(x => x.GroupId == this.Identity.Id) && IsChangedRosterInstances())
                UpdateFromInterview();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            if(@event.Instances.Any(x => x.GroupId == this.Identity.Id) && IsChangedRosterInstances())
                UpdateFromInterview();
        }

        public void Handle(YesNoQuestionAnswered @event)
        {
            if(IsChangedRosterInstances())
                UpdateFromInterview();
        }

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if(IsChangedRosterInstances())
                UpdateFromInterview();
        }

        public void Handle(QuestionsEnabled @event)
        {
            if(IsChangedRosterChildren(@event.Questions))
                UpdateFromInterview();
        }

        public void Handle(QuestionsDisabled @event)
        {
            if(IsChangedRosterChildren(@event.Questions))
                UpdateFromInterview();
        }

        public void Handle(GroupsEnabled @event)
        {
            if(IsChangedRosterChildren(@event.Groups))
                UpdateFromInterview();
        }

        public void Handle(GroupsDisabled @event)
        {
            if(IsChangedRosterChildren(@event.Groups))
                UpdateFromInterview();
        }
    }
}
