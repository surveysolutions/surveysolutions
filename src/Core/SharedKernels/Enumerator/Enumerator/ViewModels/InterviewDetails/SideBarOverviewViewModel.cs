using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarOverviewViewModel : MvxNotifyPropertyChanged, ISideBarItem
    {
        private readonly IMvxMessenger messenger;
        private NavigationState navigation;
        private bool isCurrent;
        private bool isSelected;

        public SideBarOverviewViewModel(IMvxMessenger messenger,
            DynamicTextViewModel dynamicTextViewModel,
            InterviewStateViewModel interviewStateViewModel)
        {
            this.messenger = messenger;
            this.Title = dynamicTextViewModel;
            this.SideBarGroupState = interviewStateViewModel;
        }

        public void Init(NavigationState navigationState, string interviewId)
        {
            this.navigation = navigationState;
            this.SideBarGroupState.Init(interviewId, null);
            this.Title.InitAsStatic(UIResources.Interview_Overview);

            navigationState.ScreenChanged += args => 
                this.IsSelected = this.IsCurrent = this.navigation.CurrentScreenType == ScreenType.Overview;
        }

        public void Dispose()
        {
            this.Title.Dispose();
        }

        public string Tag => "SideBar_Overview";

        public bool IsSelected
        {
            get => isSelected;
            set => this.SetProperty(ref isSelected, value);
        }

        public bool IsCurrent
        {
            get => isCurrent;
            set => this.SetProperty(ref isCurrent, value);
        }

        public bool Expanded { get; } = false;
        public int NodeDepth { get; } = 0;
        public bool HasChildren { get; } = false;

        public GroupStateViewModel SideBarGroupState { get; private set; }
        public DynamicTextViewModel Title { get; }

        public ICommand ToggleCommand => new MvxCommand(() => { });

        public ICommand NavigateToSectionCommand => new MvxAsyncCommand(this.NavigateToSection);

        private async Task NavigateToSection()
        {
            this.messenger.Publish(new SectionChangeMessage(this));
            await this.navigation.NavigateTo(NavigationIdentity.CreateForOverviewScreen());
        }
    }
}
