using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    [DebuggerDisplay("Title = {Title}, Id = {SectionIdentity}")]
    public class SideBarSectionViewModel : MvxNotifyPropertyChanged, IDisposable
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IMvxMessenger messenger;

        public DynamicTextViewModel Title { get; }

        private string interviewId;

        private SideBarSectionsViewModel root;

        public SideBarSectionViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            ISideBarSectionViewModelsFactory modelsFactory,
            IMvxMessenger messenger,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.modelsFactory = modelsFactory;
            this.messenger = messenger;

            this.Title = dynamicTextViewModel;

            this.Children = new List<SideBarSectionViewModel>();
        }

        public void Init(string interviewId,
            NavigationIdentity navigationIdentity,
            SideBarSectionsViewModel root,
            SideBarSectionViewModel parent,
            GroupStateViewModel groupStateViewModel,
            NavigationState navigationState)
        {
            this.interviewId = interviewId;
            
            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            groupStateViewModel.Init(interviewId, navigationIdentity.TargetGroup);
            this.root = root;
            this.Parent = parent;
            this.SectionIdentity = navigationIdentity.TargetGroup;
            this.HasChildren = interview.GetEnabledSubgroups(navigationIdentity.TargetGroup).Any();
            this.NodeDepth = this.UnwrapReferences(x => x.Parent).Count() - 1;
            this.IsCurrent = this.SectionIdentity.Equals(navigationState.CurrentGroup);
            this.Title.Init(interviewId, this.SectionIdentity);
            if (this.Parent != null)
            {
                this.IsSelected = this.Parent.IsSelected;
            }
           
            this.SideBarGroupState = groupStateViewModel;
            this.ScreenType = navigationIdentity.TargetScreen;
            this.NavigationState = navigationState;
            this.NavigationState.ScreenChanged += this.OnScreenChanged;
            this.NavigationState.BeforeScreenChanged += this.OnBeforeScreenChanged;
        }

        public void InitCompleteScreenItem(string interviewId, GroupStateViewModel groupStateViewModel, NavigationState navigationState)
        {
            this.InitServiceItem(interviewId, groupStateViewModel, navigationState,
                ScreenType.Complete, UIResources.Interview_Complete_Screen_Title);
        }

        public void InitCoverScreenItem(string interviewId, CoverStateViewModel coverStateViewModel, NavigationState navigationState)
        {
            this.InitServiceItem(interviewId, coverStateViewModel, navigationState, 
                ScreenType.Cover, UIResources.Interview_Cover_Screen_Title);
        }

        private void InitServiceItem(string interviewId, GroupStateViewModel coverStateViewModel,
            NavigationState navigationState, ScreenType screenType, string screenTitle)
        {
            this.interviewId = interviewId;

            this.Parent = null;
            this.HasChildren = false;
            this.NodeDepth = 0;
            this.IsCurrent = navigationState.CurrentScreenType == screenType;
            this.Title.InitAsStatic(screenTitle);
            coverStateViewModel.Init(interviewId, null);

            this.SideBarGroupState = coverStateViewModel;
            this.ScreenType = screenType;
            this.NavigationState = navigationState;
            this.NavigationState.ScreenChanged += this.OnScreenChanged;
            this.NavigationState.BeforeScreenChanged += this.OnBeforeScreenChanged;
        }

        void OnBeforeScreenChanged(BeforeScreenChangedEventArgs eventArgs)
        {
            this.IsCurrent = false;
        }

        void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (this.ScreenType != eventArgs.TargetScreen)
                return;

            switch (eventArgs.TargetScreen)
            {
                case ScreenType.Complete:
                case ScreenType.Cover:
                    this.IsCurrent = true;
                    break;
                default:
                    if (this.SectionIdentity.Equals(eventArgs.TargetGroup))
                    {
                        this.IsCurrent = true;
                        if (!this.Expanded)
                        {
                            this.Expanded = true;
                        }
                    }
                    break;
            }
        }

        public GroupStateViewModel SideBarGroupState { get; private set; }

        public NavigationState NavigationState { get; set; }
        public Identity SectionIdentity { get; set; }
        public ScreenType ScreenType { get; set; }

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
            get { return this.hasChildren; }
            set
            {
                if (this.hasChildren == value) return;
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
                        this.Children.TreeToEnumerable(x => x.Children)
                                .ToList()
                                .ForEach(x => x.Dispose());

                        this.Children = new List<SideBarSectionViewModel>();
                    }

                    this.RaisePropertyChanged();
                }
            }
        }

        public int NodeDepth { get; set; }

        private List<SideBarSectionViewModel> children;
        public List<SideBarSectionViewModel> Children
        {
            get { return this.children; }
            set
            {
                this.children = value;
                this.RaisePropertyChanged();
            }
        }
        public ICommand NavigateToSectionCommand => new MvxCommand(this.NavigateToSection);

        public ICommand Toggle
        {
            get
            {
                return new MvxCommand(() =>
                {
                    this.Expanded = !this.Expanded;
                    this.root.UpdateSideBarTree();
                });
            }
        }

        private void NavigateToSection()
        {
            this.messenger.Publish(new SectionChangeMessage(this));

            NavigationIdentity navigationIdentity;
            switch (this.ScreenType)
            {
                case ScreenType.Complete:
                    navigationIdentity = NavigationIdentity.CreateForCompleteScreen();
                    break;
                case ScreenType.Cover:
                    navigationIdentity = NavigationIdentity.CreateForCoverScreen();
                    break;
                default:
                    navigationIdentity = NavigationIdentity.CreateForGroup(this.SectionIdentity);
                    break;
            }

            this.NavigationState.NavigateTo(navigationIdentity);
        }

        private List<SideBarSectionViewModel> GenerateChildNodes()
        {
            if (this.SectionIdentity == null)
                return new List<SideBarSectionViewModel>();

            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.NavigationState.InterviewId);

            var enabledSubgroups = interview.GetEnabledSubgroups(this.SectionIdentity).ToList();

            var result = enabledSubgroups.Select(groupInstance => this.modelsFactory.BuildSectionItem(this.root, this, NavigationIdentity.CreateForGroup(groupInstance), 
                                                                                               this.NavigationState, this.NavigationState.InterviewId));

            return new List<SideBarSectionViewModel>(result);
        }

        public SideBarSectionViewModel Parent { get; set; }

        public void RemoveMe()
        {
            this.RefreshHasChildrenFlag();
            this.Dispose();
        }

        public void Dispose()
        {
            this.NavigationState.ScreenChanged -= this.OnScreenChanged;
            this.NavigationState.BeforeScreenChanged -= this.OnBeforeScreenChanged;
            this.Title.Dispose();
            this.Children?.ForEach(x => x.Dispose());
        }

        public void RefreshHasChildrenFlag()
        {
            if (ScreenType == ScreenType.Complete || ScreenType == ScreenType.Cover)
                return;

            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            this.HasChildren = interview.GetEnabledSubgroups(this.SectionIdentity).Any();
        }
    }
}