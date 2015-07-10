using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    [DebuggerDisplay("Title = {Title}, Id = {SectionIdentity}")]
    public class SideBarSectionViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<GroupsDisabled>,
        IDisposable
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IMvxMessenger messenger;

        public SideBarSectionViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher, 
            ISideBarSectionViewModelsFactory modelsFactory,
            IMvxMessenger messenger)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionService = substitutionService;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.modelsFactory = modelsFactory;
            this.messenger = messenger;
            this.Children = new ObservableCollection<SideBarSectionViewModel>();
            this.Children.CollectionChanged += (sender, args) => this.HasChildren = this.Children.Count > 0;
            eventRegistry.Subscribe(this);
        }

        public void Init(NavigationState navigationState)
        {
            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += NavigationState_OnGroupChanged;
            this.navigationState.OnBeforeGroupChanged += navigationState_OnBeforeGroupChanged;
        }

        void navigationState_OnBeforeGroupChanged(BeforeGroupChangedEventArgs eventArgs)
        {
            this.IsCurrent = false;
        }

        void NavigationState_OnGroupChanged(GroupChangedEventArgs newGroupIdentity)
        {
            if (this.SectionIdentity.Equals(newGroupIdentity.TargetGroup))
            {
                this.IsCurrent = true;
                if (!this.Expanded)
                {
                    this.Expanded = true;
                }
            }
        }

        private NavigationState navigationState;

        public Identity SectionIdentity { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected == value) return;
                this.isSelected = value;
                this.RaisePropertyChanged();
            }
        }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set
            {
                if (this.isCurrent == value) return;
                this.isCurrent = value; 
                this.RaisePropertyChanged();
            }
        }

        private bool hasChildren;
        public bool HasChildren
        {
            get { return hasChildren; }
            set
            {
                if (this.HasChildren == value) return;
                this.hasChildren = value; 
                this.RaisePropertyChanged();
            }
        }

        private bool expanded;
        public bool Expanded
        {
            get { return this.expanded; }
            set
            {
                if (this.Expanded != value)
                {
                    this.expanded = value;
                    if (this.expanded)
                    {
                        this.Children = this.GenerateChildNodes();
                    }
                    else
                    {
                        this.TreeToEnumerable(x => x.Children)
                            .Skip(1) // Skip self
                            .ForEach(x => x.Dispose());
                        this.Children.Clear();
                    }

                    this.RaisePropertyChanged();
                }
            }
        }

        public int NodeDepth { get; set; }

        public ObservableCollection<SideBarSectionViewModel> Children
        {
            get { return this.children; }
            set
            {
                this.children = value; 
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => HasChildren);
            }
        }

        private MvxCommand navigateToSectionCommand;
        private ObservableCollection<SideBarSectionViewModel> children;

        public ICommand NavigateToSectionCommand
        {
            get
            {
                this.navigateToSectionCommand = this.navigateToSectionCommand ?? new MvxCommand(async () => await this.NavigateToSection());
                return this.navigateToSectionCommand;
            }
        }

        public ICommand Toggle
        {
            get
            {
                return new MvxCommand(() => this.Expanded = !this.Expanded);
            }
        }

        private async Task NavigateToSection()
        {
            messenger.Publish(new SectionChangeMessage(this));
            await this.navigationState.NavigateTo(this.SectionIdentity);
        }

        private ObservableCollection<SideBarSectionViewModel> GenerateChildNodes()
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.navigationState.InterviewId);
            QuestionnaireModel questionnaireModel = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            IEnumerable<Identity> enabledChildGroups = interview.GetEnabledSubgroups(this.SectionIdentity);
            
            List<SideBarSectionViewModel> result = new List<SideBarSectionViewModel>();
            foreach (Identity groupInstance in enabledChildGroups)
            {
                var group = questionnaireModel.GroupsWithFirstLevelChildrenAsReferences[groupInstance.Id];
                var section = modelsFactory.BuildSectionItem(this, group, groupInstance, interview, substitutionService, navigationState);

                result.Add(section);
            }

            return new ObservableCollection<SideBarSectionViewModel>(result);
        }

        public SideBarSectionViewModel Parent { get; set; }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var myChangedInstance = @event.ChangedInstances.SingleOrDefault(x => x.RosterInstance.GetIdentity().Equals(this.SectionIdentity));
            if (myChangedInstance != null)
            {
                QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(this.navigationState.QuestionnaireId);
                string groupTitle = questionnaire.GroupsWithFirstLevelChildrenAsReferences[this.SectionIdentity.Id].Title;
                string rosterTitle = myChangedInstance.Title;

                string sectionFullName = this.substitutionService.GenerateRosterName(groupTitle, rosterTitle);
                this.Title = sectionFullName;
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            var meDisabled = @event.Groups.Any(x => new Identity(x.Id, x.RosterVector).Equals(this.SectionIdentity));
            if (meDisabled)
            {
                this.RemoveMe();
            }
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            var meRemoved = @event.Instances.Any(x => x.GetIdentity().Equals(this.SectionIdentity));
            if (meRemoved)
            {
                this.RemoveMe();
            } 
        }

        private void RemoveMe()
        {
            this.eventRegistry.Unsubscribe(this);
            this.mainThreadDispatcher.RequestMainThreadAction(() => this.Parent.Children.Remove(this));
            this.Dispose();
        }

        public void Dispose()
        {
            this.navigationState.OnGroupChanged -= this.NavigationState_OnGroupChanged;
            this.navigationState.OnBeforeGroupChanged -= this.navigationState_OnBeforeGroupChanged;
        }
    }
}