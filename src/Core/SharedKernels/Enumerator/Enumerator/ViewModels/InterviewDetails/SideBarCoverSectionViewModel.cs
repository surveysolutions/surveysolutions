using System.Diagnostics;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    [DebuggerDisplay("Cover")]
    public class SideBarCoverSectionViewModel : MvxNotifyPropertyChanged, ISideBarItem
    {
        private NavigationState navigationState;
        private readonly IMvxMessenger messenger;

        public DynamicTextViewModel Title { get; }
        public GroupStateViewModel SideBarGroupState { get; }

        public bool Expanded { get; } = false;
        public bool HasChildren { get; } = false;
        public int NodeDepth { get; } = 0; 

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set { this.RaiseAndSetIfChanged(ref this.isCurrent, value); }
        }
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.RaiseAndSetIfChanged(ref this.isSelected, value); }
        }

        public ICommand ToggleCommand => new MvxCommand(() => { });
        public ICommand NavigateToSectionCommand => new MvxCommand(this.NavigateToSection);

        public SideBarCoverSectionViewModel(
            IMvxMessenger messenger,
            DynamicTextViewModel dynamicTextViewModel,
            CoverStateViewModel coverStateViewModel)
        {
            this.messenger = messenger;
            this.Title = dynamicTextViewModel;
            this.SideBarGroupState = coverStateViewModel;
        }

        public void Init(NavigationState navigationState)
        {
            this.Title.InitAsStatic(UIResources.Interview_Cover_Screen_Title);
            
            this.navigationState = navigationState;
            this.navigationState.ScreenChanged += this.OnScreenChanged;
        }

        void OnScreenChanged(ScreenChangedEventArgs eventArgs) => this.UpdateSelection();

        private void UpdateSelection()
            => this.IsSelected = this.IsCurrent = this.navigationState.CurrentScreenType == ScreenType.Cover;

        private void NavigateToSection()
        {
            this.messenger.Publish(new SectionChangeMessage(this));
            this.navigationState.NavigateTo(NavigationIdentity.CreateForCoverScreen());
        }

        public void Dispose()
        {
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.Title.Dispose();
        }
    }
}