using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Tester.ViewModels
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
        private string interviewId;

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

        public void Init(string interviewId, 
            Identity sectionIdentity,
            SideBarSectionViewModel parent, 
            NavigationState navigationState)
        {
            this.interviewId = interviewId;

            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            var questionnaireModel = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var groupModel = questionnaireModel.GroupsWithFirstLevelChildrenAsReferences[sectionIdentity.Id];

            this.Parent = parent;
            this.SectionIdentity = sectionIdentity;
            this.HasChildren = interview.GetEnabledSubgroups(sectionIdentity).Any();
            this.NodeDepth = this.UnwrapReferences(x => x.Parent).Count() - 1;
            if (groupModel is RosterModel)
            {
                string rosterTitle = interview.GetRosterTitle(sectionIdentity);
                this.Title = substitutionService.GenerateRosterName(groupModel.Title, rosterTitle);
            }
            else
            {
                this.Title = groupModel.Title;
            }

            this.NavigationState = navigationState;
            this.NavigationState.GroupChanged += NavigationState_OnGroupChanged;
            this.NavigationState.BeforeGroupChanged += navigationState_OnBeforeGroupChanged;
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

        public NavigationState NavigationState {get; set; }
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

        private ObservableCollection<SideBarSectionViewModel> children;
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
            await this.NavigationState.NavigateTo(this.SectionIdentity);
        }

        private ObservableCollection<SideBarSectionViewModel> GenerateChildNodes()
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.NavigationState.InterviewId);

            var result = interview.GetEnabledSubgroups(this.SectionIdentity)
                                  .Select(groupInstance =>  this.modelsFactory.BuildSectionItem(this, groupInstance, this.NavigationState, this.NavigationState.InterviewId));

            return new ObservableCollection<SideBarSectionViewModel>(result);
        }

        public SideBarSectionViewModel Parent { get; set; }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var myChangedInstance = @event.ChangedInstances.SingleOrDefault(x => x.RosterInstance.GetIdentity().Equals(this.SectionIdentity));
            if (myChangedInstance != null)
            {
                QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(this.NavigationState.QuestionnaireId);
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
            this.NavigationState.GroupChanged -= this.NavigationState_OnGroupChanged;
            this.NavigationState.BeforeGroupChanged -= this.navigationState_OnBeforeGroupChanged;
        }
    }
}