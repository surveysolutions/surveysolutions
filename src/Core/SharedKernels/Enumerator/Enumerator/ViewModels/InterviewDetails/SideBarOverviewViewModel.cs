using System;
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
        private readonly AnswerNotifier answerNotifier;
        private NavigationState navigation;
        private bool isCurrent;
        private bool isSelected;

        public SideBarOverviewViewModel(IMvxMessenger messenger,
            DynamicTextViewModel dynamicTextViewModel,
            InterviewStateViewModel interviewStateViewModel,
            AnswerNotifier answerNotifier)
        {
            this.messenger = messenger;
            this.Title = dynamicTextViewModel;
            this.SideBarGroupState = interviewStateViewModel;
            this.answerNotifier = answerNotifier;
        }

        public void Init(NavigationState navigationState, string interviewId)
        {
            this.navigation = navigationState;
            this.SideBarGroupState.Init(interviewId, null);
            this.Title.InitAsStatic(UIResources.Interview_Overview);

            navigationState.ScreenChanged += OnScreenChanged;

            this.answerNotifier.Init(interviewId);
            this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs) => 
            this.IsSelected = this.IsCurrent = this.navigation.CurrentScreenType == ScreenType.Overview;

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.SideBarGroupState.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.SideBarGroupState);
        }

        public void Dispose()
        {
            this.navigation.ScreenChanged -= this.OnScreenChanged;

            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
            this.answerNotifier.Dispose();
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
